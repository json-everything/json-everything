using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using MdDox.MarkdownWriters;
using MdDox.MarkdownWriters.Interfaces;
using DocXml.Reflection;
using LoxSmoke.DocXml;
using LoxSmoke.DocXml.Reflection;
using System.ComponentModel.DataAnnotations;
using MdDox.CommandLineOptions;

namespace MdDox
{
    /// <summary>
    /// <typeparamref name=""/>
    /// <paramref name=""/>
    /// <![CDATA[]]>
    /// <c></c>
    /// <code></code>
    /// <example></example>
    /// <exception cref=""></exception>
    /// <list type=""></list>
    /// <para></para>
    /// <see cref=""/>
    /// <seealso cref=""/>
    /// </summary>

    class Program
    {
        static CommandLineOptions.CommandLineOptions Parse(string [] args)
        {
	        var options = new CommandLineOptions.CommandLineOptions();
            if (string.IsNullOrEmpty(options.Format))
            {
                options.Format = MarkdownWriters.First().FormatName;
            }
            if (options.IgnoreMethods)   options.DocumentMethodDetails = false;
            if (options.AssemblyName != null && options.OutputFile == null)
            {
                options.OutputFile = Path.GetFileNameWithoutExtension(options.AssemblyName) + ".md";
            }
            return options;
        }

        static List<IMarkdownWriter> MarkdownWriters = new List<IMarkdownWriter>()
        {
            new GithubMarkdownWriter(),
            new BitbucketMarkdownWriter()
        };
        static string MarkdownFormatNames => string.Join(",", MarkdownWriters.Select(md => md.FormatName));

        static void Main(string[] args)
        {
            var options = Parse(args);
            if (options == null)
            {
                return;
            }

            var writer = MarkdownWriters.FirstOrDefault(md => md.FormatName.Equals(options.Format, StringComparison.OrdinalIgnoreCase));

            if (options.Format == null)
            {
                writer = MarkdownWriters.First();
                Console.WriteLine($"Markdown format not specified. Assuming {writer.FormatName}.");
            }
            if (writer == null)
            {
                Console.WriteLine($"Error: invalid markdown format specified. Valid values: {MarkdownFormatNames}");
                return;
            }

            try
            {
                if (!File.Exists(options.AssemblyName)) throw new FileNotFoundException("File not found", options.AssemblyName);

                var fullAssemblyName = Path.GetFullPath(options.AssemblyName);
                if (options.Verbose) Console.WriteLine($"Document full assembly file name: \"{fullAssemblyName}\"");

                if (options.Verbose) AppDomain.CurrentDomain.AssemblyLoad += ShowAssemblyLoaded;
                AppDomain.CurrentDomain.AssemblyResolve += 
                    (sender, args) => ResolveAssembly(sender, args, options.Verbose, Path.GetDirectoryName(fullAssemblyName));

                var myAssembly = Assembly.LoadFile(fullAssemblyName);
                if (myAssembly == null)
                {
                    throw new Exception($"Could not load assembly \'{options.AssemblyName}\'");
                }

                Type rootType = null;
                if (options.TypeName != null)
                {
                    rootType = myAssembly.DefinedTypes.FirstOrDefault(t => t.Name == options.TypeName);
                    if (rootType == null)
                    {
                        var possibleTypes = myAssembly.DefinedTypes
                            .Where(t => t.Name.Contains(options.TypeName, StringComparison.OrdinalIgnoreCase))
                            .Select(t => t.Name).ToList();
                        if (possibleTypes.Count == 0)
                            throw new Exception(
                                $"Specified type name \'{options.TypeName}\' not found in assembly \'{options.AssemblyName}\'");

                        throw new Exception(
                            $"Specified type name \'{options.TypeName}\' not found in assembly \'{options.AssemblyName}\'." +
                            $" Similar type names in the assembly: {string.Join(",", possibleTypes)}");
                    }
                }
                var recursive = options.AllRecursive || options.RecursiveAssemblies.Any();
                if (options.AllRecursive) options.RecursiveAssemblies = new List<string>();

                var msdnLinks = !string.IsNullOrEmpty(options.MsdnLinkViewParameter);
                var msdnView = options.MsdnLinkViewParameter;
                if (msdnLinks && msdnView.Equals("default", StringComparison.OrdinalIgnoreCase))
                {
                    msdnView = null;
                }
                var assembly = rootType == null ? myAssembly : null;
                var typeList = OrderedTypeList.LoadTypes(
                    rootType, 
                    assembly, 
                    recursive, 
                    options.RecursiveAssemblies.ToList(), 
                    options.IgnoreAttributes.ToList(), 
                    options.IgnoreMethods, 
                    options.Verbose);

                DocumentationGenerator.GenerateMarkdown(
                    typeList,
                    GenerateTitle(assembly, options.DocumentTitle),
                    !options.DoNotShowDocumentDateTime,
                    options.DocumentMethodDetails,
                    msdnLinks, 
                    msdnView,
                    writer);

                // Write markdown to the output file
                File.WriteAllText(options.OutputFile, writer.FullText);
            }
            catch (BadImageFormatException exc)
            {
                Console.WriteLine($"Error: {exc.Message}");
                Console.WriteLine($"Hresult:{exc.HResult}");
                if (!exc.HelpLink.IsNullOrEmpty()) Console.WriteLine($"Help link: {exc.HelpLink}");
                Console.WriteLine($"{exc.StackTrace}");
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Error: {exc.Message}");
                Console.WriteLine($"{exc.StackTrace}");
            }
        }

        protected static string GenerateTitle(Assembly assembly, string format)
        {
            if (format == null && assembly == null) return null;
            var assemblyName = assembly == null ? "" : Path.GetFileName(assembly.ManifestModule.Name);
            var version = assembly == null ? "" : ("v." + assembly.GetName().Version);
            if (format == null) format = "{assembly} {version} API documentation";
            return format.Replace("{assembly}", assemblyName).Replace("{version}", version);
        }

        private static Dictionary<string, Assembly> RequestedAssemblies { get; set; } = new Dictionary<string, Assembly>();

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args, bool verbose, string basePath)
        {
            var shortAssemblyName = args.Name.Split(',').First();

            // Avoid stack overflow
            if (RequestedAssemblies.ContainsKey(shortAssemblyName)) return RequestedAssemblies[shortAssemblyName];
            RequestedAssemblies.Add(shortAssemblyName, null);
            var fullAssemblyName = Path.GetFullPath(args.Name.Split(',').First() + ".dll", basePath);

            if (verbose)
            {
                Console.WriteLine($"Resolving: {args.Name}");
                if (args.RequestingAssembly != null)
                {
                    Console.WriteLine($"Requested by: {args.RequestingAssembly}");
                    Console.WriteLine($"Requested by: {args.RequestingAssembly.Location}");
                }
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Console.WriteLine("  Already loaded: " + a.FullName);
                }
                Console.WriteLine("Loading: " + fullAssemblyName);
            }
            var assembly = Assembly.LoadFile(fullAssemblyName);
            RequestedAssemblies[shortAssemblyName] = assembly;
            return assembly;
        }

        private static void ShowAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            Console.WriteLine("Loaded assembly: " + args.LoadedAssembly.FullName);
        }
    }
}
