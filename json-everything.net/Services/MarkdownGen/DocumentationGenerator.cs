using System.Reflection;
using System.Text.RegularExpressions;
using JsonEverythingNet.Services.MarkdownGen.MarkdownWriters.Interfaces;
using JsonEverythingNet.Services.MarkdownGen.Reflection;

namespace JsonEverythingNet.Services.MarkdownGen
{
    public class DocumentationGenerator
    {
        /// <summary>
        /// The ordered list of types to document.
        /// </summary>
        public OrderedTypeList TypeList;
        /// <summary>
        /// XML documentation reader. Finds compiler-generated documentation based on type reflection information.
        /// </summary>
        public DocXmlReader Reader { get; }
        /// <summary>
        /// Writes repository-specific markdown output.
        /// </summary>
        public IMarkdownWriter Writer { get; }
        private Func<Type, Queue<string>, string> typeLinkConverter;
        private bool DocumentMethodDetails { get; set; }

        public static void GenerateMarkdown(
            OrderedTypeList typeList,
            string documentTitle,
            bool documentMethodDetails,
            bool msdnLinks,
            string msdnLinkViewParameter,
            IMarkdownWriter markdownWriter)
        {
            // Generate markdown
            var generator = new DocumentationGenerator(markdownWriter, typeList, msdnLinks, msdnLinkViewParameter, documentMethodDetails);
            if (documentTitle != null) generator.WriteDocumentTitle(documentTitle);
            generator.WriteTypeIndex();
            generator.WriteDocumentationForTypes();
        }

        public static void GenerateMarkdown(
            Type type,
            IMarkdownWriter markdownWriter)
        {
	        var typeList = new OrderedTypeList(type);
			var generator = new DocumentationGenerator(markdownWriter, typeList, documentMethodDetails: true);
            var typeInfo = typeList.TypesToDocument.First(x => x.Type == type);
            generator.WriteDocumentationForType(typeInfo);
        }

		public DocumentationGenerator(
            IMarkdownWriter writer,
            OrderedTypeList typeList,
            bool msdnLinks = false,
            string msdnView = null,
            bool documentMethodDetails = false)
        {
            Reader = new DocXmlReader();
            Writer = writer;
			TypeList = typeList;

			typeLinkConverter = (type, _) => TypeNameWithLinks(type, msdnLinks, msdnView);
            DocumentMethodDetails = documentMethodDetails;
        }

        public string TypeNameWithLinks(Type type, bool msdnLinks, string msdnView)
        {
            if (TypeList.TypesToDocumentSet.Contains(type))
            {
                return type.IsGenericTypeDefinition ?
                    Writer.HeadingLink(TypeTitle(type), type.Name.CleanGenericTypeName()) :
                    Writer.HeadingLink(TypeTitle(type), type.ToNameString());
            }
            if (msdnLinks &&
                type != typeof(string) &&
                (!type.IsValueType || type.IsEnum) &&
                (type.Assembly.ManifestModule.Name.StartsWith("System.") ||
                type.Assembly.ManifestModule.Name.StartsWith("Microsoft.")))
            {
                return Writer.Link(MsdnUrlForType(type, msdnView),
                    type.IsGenericTypeDefinition ? type.Name.CleanGenericTypeName() : type.ToNameString());
            }
            if (type.IsGenericTypeDefinition)
            {
                return $"{type.Name.CleanGenericTypeName()}";
            }
            return null;
        }

        public void WriteDocumentTitle(string titleText)
        {
            Writer.WriteH1(titleText ?? "");
        }

        public void WritedDateLine()
        {
            Writer.Write("Created by ");
            Writer.WriteLink("https://github.com/loxsmoke/mddox", "mddox");
            Writer.WriteLine($" on {DateTime.Now.ToShortDateString()}");
        }

        static string TypeTitle(Type type)
        {
            return type.ToNameString() + (type.IsEnum ? " Enum" : (type.IsValueType ? " Struct" : " Class"));
        }

        static (string cref, string innerText, string beforeText, string afterText) FindTagWithAttribute(
            string text, string tag, string attributeName)
        {
            if (text.IsNullOrEmpty() || !text.Contains(tag)) return (null, null, text, null);
            var simpleTag = new Regex("<" + tag + "( +)" + attributeName + "( *)=( *)\"(.*?)\"( *)/>");
            var match = simpleTag.Match(text);
            if (match.Success)
            {
                return (match.Groups[4].Value, "", text.Substring(0, match.Index),
                    text.Substring(match.Index + match.Length));
            }

            var bigTag = new Regex("<" + tag + "( +)"+ attributeName + "( *)=( *)\"(.*?)\"( *)>(.*?)</" + tag + ">");
            match = bigTag.Match(text);
            if (match.Success)
            {
                return (match.Groups[4].Value, match.Groups[6].Value, text.Substring(0, match.Index),
                    text.Substring(match.Index + match.Length));
            }
            return (null, null, text, null);
        }

        string ProcessTags(string text)
        {
            for (; ; )
            {
                var (cref, innerText, beforeText, afterText) = FindTagWithAttribute(text, "seealso", "cref");
                if (cref != null)
                {
                    text = beforeText + Writer.Bold(FixCref(cref)) + afterText;
                    continue;
                }
                (cref, innerText, beforeText, afterText) = FindTagWithAttribute(text, "see", "cref");
                if (cref != null)
                {
                    text = beforeText + Writer.Bold(FixCref(cref)) + afterText;
                    continue;
                }
                (cref, innerText, beforeText, afterText) = FindTagWithAttribute(text, "see", "href");
                if (cref != null)
                {
                    text = beforeText + $" {Writer.Link(cref, innerText )} " + afterText;
                    continue;
                }

                text = RemoveParaTags(text);
                text = RemoveCodeTags(text);
                return text;
            }
        }

        static string FixCref(string crefText)
        {
            if (crefText.Contains(":")) // XML doc Id
            {
                return crefText.Substring(crefText.IndexOf(":") + 1);
            }
            return crefText;
        }

        /// <summary>
        /// Generate URL to the documentation page of the type at https://docs.microsoft.com/
        /// </summary>
        /// <param name="type">The type to generate url for</param>
        /// <param name="view">The documentation framework version parameter.
        /// For example netcore-3.1, netframework-4.8, netstandard-2.1, and so on.
        /// If not specified then view parameter is omitted.</param>
        /// <returns>URL to the type documentation page</returns>
        static string MsdnUrlForType(Type type, string view = null)
        {
            var docLocale = "en-us";
            var urlParameters = view.IsNullOrEmpty() ? "" : $"?view={view}";
            var typeNameFragment = type.FullName.ToLowerInvariant();
            if (typeNameFragment.Contains('`')) typeNameFragment = typeNameFragment.Replace('`', '-');
            var url = $"https://docs.microsoft.com/{docLocale}/dotnet/api/{typeNameFragment}{urlParameters}";
            return url;
        }

        static string DoubleNewLine = Environment.NewLine + Environment.NewLine;

        static string RemoveParaTags(string text) => text?
            .RegexReplace(@"\s*</para>\s*<para>\s*", DoubleNewLine)
            .RegexReplace(@"\s*<para>\s*", DoubleNewLine)
            .RegexReplace(@"\s*</para>\s*", DoubleNewLine)
            .Trim();

        static string InlineCode = "`";
        static string MultilineCode = "```" + Environment.NewLine;
        static string RemoveCodeTags(string text) => text?
            .Replace("<c>", InlineCode)
            .Replace("</c>", InlineCode)
            .Replace("<code>", MultilineCode)
            .Replace("</code>", Environment.NewLine + MultilineCode)
            .Trim();

        /// <summary>
        /// Write table of contents. It is a three column table with each cell containing 
        /// the link to the heading of the type.
        /// </summary>
        /// <param name="indexTitleText"></param>
        public void WriteTypeIndex(string indexTitleText = "All types")
        {
            var namesForTOC = TypeList.TypesToDocument
                .Select(typeData => Writer.HeadingLink(TypeTitle(typeData.Type), TypeTitle(typeData.Type))).ToList();
            if (namesForTOC.Count == 0) return;

            if (indexTitleText != null) Writer.WriteH1(indexTitleText);
            Writer.WriteTableTitle(" ", " ", " ");
            var rowCount = namesForTOC.Count / 3 + (((namesForTOC.Count % 3) == 0) ? 0 : 1);
            for (var i = 0; i < rowCount; i++)
            {
                Writer.WriteTableRow(namesForTOC[i],
                    rowCount + i < namesForTOC.Count ? namesForTOC[rowCount + i] : " ",
                    rowCount * 2 + i < namesForTOC.Count ? namesForTOC[rowCount * 2 + i] : " ");
            }
        }

        /// <summary>
        /// Write markdown documentation for the enum type:
        /// Examples, Remarks, 
        /// </summary>
        /// <param name="enumType"></param>
        public void WriteEnumDocumentation(Type enumType)
        {
            Writer.WriteH1(TypeTitle(enumType));
            Writer.WriteLine("Namespace: " + enumType.Namespace);

            var enumComments = Reader.GetEnumComments(enumType, true);
            Writer.WriteLine(ProcessTags(enumComments.Summary));

            WriteExample(enumComments.Example);
            WriteRemarks(enumComments.Remarks);

            if (enumComments.ValueComments.Count > 0)
            {
                Writer.WriteH2("Values");
                Writer.WriteTableTitle("Name", "Summary");
                foreach (var prop in enumComments.ValueComments)
                {
                    Writer.WriteTableRow(Writer.Bold(prop.Name),
                        ProcessTags(prop.Summary));
                }
            }
        }

        /// <summary>
        /// Write markdown documentation for the class:
        /// Base class,  summary, remarks, Properties, constructors, methods and fields
        /// </summary>
        /// <param name="typeData"></param>
        public void WriteClassDocumentation(TypeCollection.TypeInformation typeData)
        {
            Writer.WriteH1(TypeTitle(typeData.Type));
            Writer.WriteLine("Namespace: " + typeData.Type.Namespace);

            if (typeData.Type.BaseType != null &&
                typeData.Type.BaseType != typeof(Object) &&
                typeData.Type.BaseType != typeof(ValueType))
            {
                Writer.WriteLine("Base class: " + typeData.Type.BaseType.ToNameString(typeLinkConverter, true));
            }

            var typeComments = Reader.GetTypeComments(typeData.Type);
            Writer.WriteLine(ProcessTags(typeComments.Summary));

            WriteExample(typeComments.Example);
            WriteRemarks(typeComments.Remarks);

            var allProperties = Reader.Comments(typeData.Properties).ToList();
            var allConstructors = Reader.Comments(typeData.Methods.Where(it => it is ConstructorInfo)).ToList();
            var allMethods = Reader.Comments(typeData.Methods
                .Where(it => !(it is ConstructorInfo) && (it is MethodInfo))).ToList();
            var allFields = Reader.Comments(typeData.Fields).ToList();
            if (allProperties.Count > 0)
            {
                Writer.WriteH2("Properties");
                Writer.WriteTableTitle("Name", "Type", "Summary");
                foreach (var prop in allProperties)
                {
                    Writer.WriteTableRow(
                        Writer.Bold(prop.Info.Name),
                        prop.Info.ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(prop.Comments.Summary));
                }
            }

            if (allConstructors.Count > 0)
            {
                Writer.WriteH2("Constructors");
                Writer.WriteTableTitle("Name", "Summary");
                foreach (var ctor in allConstructors.OrderBy(m => m.Info.GetParameters().Length))
                {
                    var heading = typeData.Type.ToNameString() + ctor.Info.ToParametersString();
                    heading = DocumentMethodDetails ? Writer.HeadingLink(heading, Writer.Bold(heading)) : Writer.Bold(heading);
                    Writer.WriteTableRow(
                        heading,
                        ProcessTags(ctor.Comments.Summary));
                }
            }

            if (allMethods.Count > 0)
            {
                Writer.WriteH2("Methods");
                Writer.WriteTableTitle("Name", "Returns", "Summary");
                foreach (var method in allMethods
                    .OrderBy(m => m.Info.Name)
                    .ThenBy(m => m.Info.GetParameters().Length))
                {
                    var methodInfo = method.Info as MethodInfo;
                    var heading = methodInfo.Name + methodInfo.ToParametersString();
                    heading = DocumentMethodDetails ? Writer.HeadingLink(heading, Writer.Bold(heading)) : Writer.Bold(heading);
                    Writer.WriteTableRow(
                        heading,
                        methodInfo.ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(method.Comments.Summary));
                }
            }

            if (allFields.Count > 0)
            {
                Writer.WriteH2("Fields");
                Writer.WriteTableTitle("Name", "Type", "Summary");
                foreach (var field in allFields)
                {
                    Writer.WriteTableRow(
                        Writer.Bold(field.Info.Name),
                        field.Info.ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(field.Comments.Summary));
                }
            }

            if (DocumentMethodDetails)
            {
                if (allConstructors.Count > 0)
                {
                    Writer.WriteH2("Constructors");
                    foreach (var (info, comments) in allConstructors
                        .OrderBy(m => m.Info.GetParameters().Length))
                    {
                        WriteMethodDetails(typeData.Type.ToNameString(), info, comments);
                    }
                }
                if (allMethods.Count > 0)
                {
                    Writer.WriteH2("Methods");
                    foreach (var (info, comments) in allMethods
                        .OrderBy(m => m.Info.Name)
                        .ThenBy(m => m.Info.GetParameters().Length))
                    {
                        WriteMethodDetails(info.Name, info, comments);
                    }
                }
            }
        }

        private void WriteMethodDetails(string name, MethodBase info, MethodComments comments)
        {
            Writer.WriteH3(name + info.ToParametersString());
            Writer.WriteLine(ProcessTags(comments.Summary));
            if (comments.Parameters.Count > 0)
            {
                var parameters = info.GetParameters();
                var i = 0;
                Writer.WriteTableTitle("Parameter", "Type", "Description");
                foreach (var (paramName, text) in comments.Parameters)
                {
                    Writer.WriteTableRow(paramName,
                        parameters[i++].ToTypeNameString(typeLinkConverter, true),
                        ProcessTags(text));
                }
            }
            Writer.WriteLine("");

            if (info is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void))
            {
                Writer.WriteH3("Returns");
                Writer.WriteLine(methodInfo.ToTypeNameString(typeLinkConverter, true));
                Writer.WriteLine(ProcessTags(comments.Returns));
            }
            WriteExample(comments.Example);
        }

        public void WriteDocumentationForTypes()
        {
            foreach (var typeData in TypeList.TypesToDocument)
            {
	            WriteDocumentationForType(typeData);
            }
        }

        private void WriteDocumentationForType(TypeCollection.TypeInformation typeData)
        {
	        if (typeData.Type.IsEnum)
	        {
		        WriteEnumDocumentation(typeData.Type);
		        return;
	        }

	        WriteClassDocumentation(typeData);
        }

        public void WriteExample(string example)
        {
            if (example.IsNullOrEmpty()) return;

            Writer.WriteH2("Examples");
            Writer.WriteLine(ProcessTags(example));
        }
        public void WriteRemarks(string remarks)
        {
            if (remarks.IsNullOrEmpty()) return;

            Writer.WriteH2("Remarks");
            Writer.WriteLine(ProcessTags(remarks));
        }
    }
}
