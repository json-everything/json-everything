using System.Reflection;
using JsonEverythingNet.Services.MarkdownGen.Reflection;

namespace JsonEverythingNet.Services.MarkdownGen
{
    public class OrderedTypeList
    {
        public TypeCollection TypeCollection { get; set; }
        public List<TypeCollection.TypeInformation> TypesToDocument { get; set; }
        public HashSet<Type> TypesToDocumentSet { get; set; }

        public OrderedTypeList(Type type)
        {
			TypeCollection = TypeCollection.ForReferencedTypes(type);
			TypesToDocument = TypeCollection.ReferencedTypes.Values
				.OrderBy(t => t.Type.Namespace)
				.ThenBy(t => t.Type.Name).ToList();
			TypesToDocumentSet = new HashSet<Type> { type };
        }

        public OrderedTypeList(TypeCollection typeCollection, Type firstType = null)
        {
            TypeCollection = typeCollection;
            TypesToDocument = typeCollection.ReferencedTypes.Values
                .OrderBy(t => t.Type.Namespace)
                .ThenBy(t => t.Type.Name).ToList();
            TypesToDocumentSet = new HashSet<Type>(TypesToDocument.Select(t => t.Type));

            if (firstType != null)
            {
                var typeDesc = TypesToDocument.FirstOrDefault(t => t.Type == firstType);
                if (typeDesc != null)
                {
                    TypesToDocument.Remove(typeDesc);
                    TypesToDocument.Insert(0, typeDesc);
                }
            }
        }

        public static OrderedTypeList LoadTypes(
            Type rootType,
            Assembly assembly,
            bool recursiveAssemblyTraversal,
            List<string> recursiveAssemblies,
            List<string> ignoreAttributes,
            bool ignoreMethods,
            bool verbose)
        {
            // Reflection setup
            var allAssemblyTypes = assembly != null;
            if (assembly == null) assembly = rootType.Assembly;
            var ignoreAttributesSet = ignoreAttributes == null || ignoreAttributes.Count == 0 ? null : 
                new HashSet<string>(ignoreAttributes.Select(a => a.EndsWith("Attribute") ? a : (a + "Attribute")));

            if (recursiveAssemblies != null && recursiveAssemblies.Count == 0) recursiveAssemblies = null;

            if (verbose)
            {
                if (assembly != null) Log(assembly, "Root assembly ");
            }

            var reflectionSettings = ReflectionSettings.Default;
            reflectionSettings.PropertyFilter = info => PropertyFilter(info, ignoreAttributesSet, verbose);
            reflectionSettings.MethodFilter = info => MethodFilter(info, ignoreMethods, ignoreAttributesSet, verbose);
            reflectionSettings.TypeFilter = type => TypeFilter(type, ignoreAttributesSet, verbose);
            reflectionSettings.AssemblyFilter =
                reflectionAssembly => AssemblyFilter(reflectionAssembly, assembly, recursiveAssemblies, recursiveAssemblyTraversal, verbose);

            // Reflection
            var typeCollection = allAssemblyTypes ?
                TypeCollection.ForReferencedTypes(assembly, reflectionSettings) :
                TypeCollection.ForReferencedTypes(rootType, reflectionSettings);
            return new OrderedTypeList(typeCollection, rootType);
        }

        static bool HasIgnoreAttribute(PropertyInfo info, HashSet<string> ignoreAttributes)
        {
            if (ignoreAttributes == null) return false;
            return HasIgnoreAttribute(info.GetCustomAttributes(), ignoreAttributes);
        }
        static bool HasIgnoreAttribute(MethodBase info, HashSet<string> ignoreAttributes)
        {
            if (ignoreAttributes == null) return false;
            return HasIgnoreAttribute(info.GetCustomAttributes(), ignoreAttributes);
        }
        static bool HasIgnoreAttribute(Type info, HashSet<string> ignoreAttributes)
        {
            if (ignoreAttributes == null) return false;
            return HasIgnoreAttribute(info.GetCustomAttributes(), ignoreAttributes);
        }

        static bool HasIgnoreAttribute(IEnumerable<Attribute> customAttributes, HashSet<string> ignoreAttributes)
        {
            var attributeList = customAttributes.ToList();
            if (attributeList.Count == 0) return false;
            return attributeList.Any(attr => ignoreAttributes.Contains(attr.GetType().Name));
        }

        #region Filters and logging
        public static bool PropertyFilter(PropertyInfo info, HashSet<string> ignoreAttributesSet, bool verbose)
        {
            var document = !HasIgnoreAttribute(info, ignoreAttributesSet);
            if (verbose)
            {
                Log(info, (document ? "Document " : "Ignore by attribute ") + "property ");
            }
            return document;
        }

        public static void Log(PropertyInfo info, string message)
        {
            Console.WriteLine("    " + message + info.ToTypeNameString() + " " + info.Name);
        }

        public static bool MethodFilter(MethodBase info, bool ignoreMethods, HashSet<string> ignoreAttributesSet, bool verbose)
        {
            if (ignoreMethods) return false;
            var document = !HasIgnoreAttribute(info, ignoreAttributesSet);
            if (verbose)
            {
                Log(info, (document ? "Document " : "Ignore by attribute ") + "method ");
            }
            return document;
        }

        public static void Log(MethodBase info, string message)
        {
            Console.WriteLine("    " + message + info.Name + info.ToParametersString());
        }

        public static bool TypeFilter(Type type, HashSet<string> ignoreAttributesSet, bool verbose)
        {
            if (HasIgnoreAttribute(type, ignoreAttributesSet))
            {
                if (verbose) Log(type, "Ignore by attribute ");
                return false;
            }
            if (verbose) Log(type, "Document type ");
            return true;
        }

        public static void Log(Type type, string message)
        {
            Console.WriteLine("  " + message + type.Namespace + "." + type.ToNameString());
        }

        public static bool AssemblyFilter(
            Assembly assembly,
            Assembly rootAssembly,
            List<string> recursiveAssemblies,
            bool recursiveAssemblyTraversal,
            bool verbose)
        {
            if (assembly == rootAssembly) return true;

            if (!recursiveAssemblyTraversal)
            {
                if (!verbose) return false;
                Log(assembly, "No recursive traversal. Ignoring ");
                return false;
            }

            if (recursiveAssemblies == null) return true;

            if (!recursiveAssemblies.Any(name => name.Equals(Path.GetFileName(assembly.Location), StringComparison.OrdinalIgnoreCase)))
            {
                if (!verbose) return false;
                Log(assembly, "Assembly not in the list. Ignoring ");
                return false;
            }
            if (File.Exists(Path.ChangeExtension(assembly.Location, ".xml"))) return true;
            if (!verbose) return false;
            Log(assembly, "No xml file for the assembly. Ignoring ");
            return false;
        }

        public static void Log(Assembly assembly, string message)
        {
            Console.WriteLine(message + assembly.FullName);
            Console.WriteLine("File path: " + assembly.Location);
        }
        #endregion
    }
}
