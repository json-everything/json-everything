using System.Reflection;
using System.Text.RegularExpressions;
using JsonEverythingNet.Services.MarkdownGen.Reflection;

namespace JsonEverythingNet.Services.MarkdownGen;

public class DocumentationGenerator
{
	private static readonly string _doubleNewLine = Environment.NewLine + Environment.NewLine;
	private static readonly string _inlineCode = "`";
	private static readonly string _multilineCode = "```" + Environment.NewLine;
	private readonly Func<Type, Queue<string?>?, string?> _typeLinkConverter;

	private readonly OrderedTypeList _typeList;

	private DocumentationGenerator(
		IMarkdownWriter writer,
		OrderedTypeList typeList,
		Func<Assembly, string> getXmlCommentsFileName,
		HttpClient client)
	{
		_reader = new DocXmlReader(getXmlCommentsFileName, client);
		_writer = writer;
		_typeList = typeList;

		_typeLinkConverter = (type, _) => TypeNameWithLinks(type, false, null);
		_documentMethodDetails = false;
	}

	private readonly DocXmlReader _reader;

	private readonly IMarkdownWriter _writer;

	private readonly bool _documentMethodDetails;

	public static async Task GenerateMarkdown(
		Type type,
		IMarkdownWriter markdownWriter,
		HttpClient client)
	{
		string GetXmlCommentsFileName(Assembly assembly)
		{
			return $"{client.BaseAddress}/xml/{assembly.GetName().Name}.xml";
		}

		var typeList = new OrderedTypeList(type);
		var generator = new DocumentationGenerator(markdownWriter, typeList, GetXmlCommentsFileName, client);
		var typeInfo = typeList.TypesToDocument.First(x => x.Type == type);
		await generator.WriteDocumentationForType(typeInfo);
	}

	private string? TypeNameWithLinks(Type type, bool msdnLinks, string? msdnView)
	{
		if (_typeList.TypesToDocumentSet.Contains(type))
			return type.IsGenericTypeDefinition ? _writer.HeadingLink(TypeTitle(type), type.Name.CleanGenericTypeName()) : _writer.HeadingLink(TypeTitle(type), type.ToNameString());
		if (msdnLinks &&
		    type != typeof(string) &&
		    (!type.IsValueType || type.IsEnum) &&
		    (type.Assembly.ManifestModule.Name.StartsWith("System.") ||
		     type.Assembly.ManifestModule.Name.StartsWith("Microsoft.")))
			return _writer.Link(MsdnUrlForType(type, msdnView),
				type.IsGenericTypeDefinition ? type.Name.CleanGenericTypeName() : type.ToNameString());
		if (type.IsGenericTypeDefinition) return $"{type.Name.CleanGenericTypeName()}";
		return null;
	}

	private static string TypeTitle(Type type)
	{
		return type.ToNameString() + (type.IsEnum ? " Enum" : type.IsValueType ? " Struct" : " Class");
	}

	private static (string? cref, string? innerText, string? beforeText, string? afterText) FindTagWithAttribute(
		string? text, string tag, string attributeName)
	{
		if (text.IsNullOrEmpty() || !text!.Contains(tag)) return (null, null, text, null);
		var simpleTag = new Regex("<" + tag + "( +)" + attributeName + "( *)=( *)\"(.*?)\"( *)/>");
		var match = simpleTag.Match(text);
		if (match.Success)
			return (match.Groups[4].Value, "", text.Substring(0, match.Index),
				text[(match.Index + match.Length)..]);

		var bigTag = new Regex("<" + tag + "( +)" + attributeName + "( *)=( *)\"(.*?)\"( *)>(.*?)</" + tag + ">");
		match = bigTag.Match(text);
		if (match.Success)
			return (match.Groups[4].Value, match.Groups[6].Value, text.Substring(0, match.Index),
				text[(match.Index + match.Length)..]);
		return (null, null, text, null);
	}

	private string? ProcessTags(string? text)
	{
		for (;;)
		{
			var (cref, _, beforeText, afterText) = FindTagWithAttribute(text, "seealso", "cref");
			if (cref != null)
			{
				text = beforeText + _writer.Bold(FixCref(cref)) + afterText;
				continue;
			}

			(cref, _, beforeText, afterText) = FindTagWithAttribute(text, "see", "cref");
			if (cref != null)
			{
				text = beforeText + _writer.Bold(FixCref(cref)) + afterText;
				continue;
			}

			(cref, var innerText, beforeText, afterText) = FindTagWithAttribute(text, "see", "href");
			if (cref != null)
			{
				text = beforeText + $" {_writer.Link(cref, innerText)} " + afterText;
				continue;
			}

			text = RemoveParaTags(text);
			text = RemoveCodeTags(text);
			return text;
		}
	}

	private static string FixCref(string crefText)
	{
		if (crefText.Contains(':')) // XML doc Id
			return crefText[(crefText.IndexOf(":", StringComparison.Ordinal) + 1)..];
		return crefText;
	}

	/// <summary>
	///     Generate URL to the documentation page of the type at https://docs.microsoft.com/
	/// </summary>
	/// <param name="type">The type to generate url for</param>
	/// <param name="view">
	///     The documentation framework version parameter.
	///     For example netcore-3.1, netframework-4.8, netstandard-2.1, and so on.
	///     If not specified then view parameter is omitted.
	/// </param>
	/// <returns>URL to the type documentation page</returns>
	private static string MsdnUrlForType(Type type, string? view = null)
	{
		var docLocale = "en-us";
		var urlParameters = view.IsNullOrEmpty() ? "" : $"?view={view}";
		var typeNameFragment = type.FullName!.ToLowerInvariant();
		if (typeNameFragment.Contains('`')) typeNameFragment = typeNameFragment.Replace('`', '-');
		var url = $"https://docs.microsoft.com/{docLocale}/dotnet/api/{typeNameFragment}{urlParameters}";
		return url;
	}

	private static string? RemoveParaTags(string? text)
	{
		return text?
			.RegexReplace(@"\s*</para>\s*<para>\s*", _doubleNewLine)
			.RegexReplace(@"\s*<para>\s*", _doubleNewLine)
			.RegexReplace(@"\s*</para>\s*", _doubleNewLine)
			.Trim();
	}

	private static string? RemoveCodeTags(string? text)
	{
		return text?
			.Replace("<c>", _inlineCode)
			.Replace("</c>", _inlineCode)
			.Replace("<code>", _multilineCode)
			.Replace("</code>", Environment.NewLine + _multilineCode)
			.Trim();
	}

	/// <summary>
	///     Write markdown documentation for the enum type:
	///     Examples, Remarks,
	/// </summary>
	/// <param name="enumType"></param>
	private async Task WriteEnumDocumentation(Type enumType)
	{
		_writer.WriteH1(TypeTitle(enumType));
		_writer.WriteLine("Namespace: " + enumType.Namespace);

		var enumComments = await _reader.GetEnumComments(enumType, true);
		_writer.WriteLine(ProcessTags(enumComments.Summary!));

		WriteExample(enumComments.Example);
		WriteRemarks(enumComments.Remarks);

		if (enumComments.ValueComments.Count > 0)
		{
			_writer.WriteH2("Values");
			_writer.WriteTableTitle("Name", "Summary");
			foreach (var prop in enumComments.ValueComments)
				_writer.WriteTableRow(_writer.Bold(prop.Name),
					ProcessTags(prop.Summary));
		}
	}

	/// <summary>
	///     Write markdown documentation for the class:
	///     Base class,  summary, remarks, Properties, constructors, methods and fields
	/// </summary>
	/// <param name="typeData"></param>
	private async Task WriteClassDocumentation(TypeCollection.TypeInformation typeData)
	{
		_writer.WriteH1(TypeTitle(typeData.Type));
		_writer.WriteLine("Namespace: " + typeData.Type.Namespace);

		if (typeData.Type.BaseType != null &&
		    typeData.Type.BaseType != typeof(object) &&
		    typeData.Type.BaseType != typeof(ValueType))
			_writer.WriteLine("Base class: " + typeData.Type.BaseType.ToNameString(_typeLinkConverter, true));

		var typeComments = await _reader.GetTypeComments(typeData.Type);
		_writer.WriteLine(ProcessTags(typeComments.Summary));

		WriteExample(typeComments.Example);
		WriteRemarks(typeComments.Remarks);

		var allProperties = (await _reader.Comments(typeData.Properties)).ToList();
		var allConstructors = (await _reader.Comments(typeData.Methods.Where(it => it is ConstructorInfo))).ToList();
		var allMethods = (await _reader.Comments(typeData.Methods
			.Where(it => !(it is ConstructorInfo) && it is MethodInfo))).ToList();
		var allFields = (await _reader.Comments(typeData.Fields)).ToList();
		if (allProperties.Count > 0)
		{
			_writer.WriteH2("Properties");
			_writer.WriteTableTitle("Name", "Type", "Summary");
			foreach (var prop in allProperties)
				_writer.WriteTableRow(
					_writer.Bold(prop.Info.Name),
					prop.Info.ToTypeNameString(_typeLinkConverter, true),
					ProcessTags(prop.Comments.Summary));
		}

		if (allConstructors.Count > 0)
		{
			_writer.WriteH2("Constructors");
			_writer.WriteTableTitle("Name", "Summary");
			foreach (var ctor in allConstructors.OrderBy(m => m.Info.GetParameters().Length))
			{
				var heading = typeData.Type.ToNameString() + ctor.Info.ToParametersString();
				heading = _documentMethodDetails ? _writer.HeadingLink(heading, _writer.Bold(heading)) : _writer.Bold(heading);
				_writer.WriteTableRow(
					heading,
					ProcessTags(ctor.Comments.Summary));
			}
		}

		if (allMethods.Count > 0)
		{
			_writer.WriteH2("Methods");
			_writer.WriteTableTitle("Name", "Returns", "Summary");
			foreach (var method in allMethods
				         .OrderBy(m => m.Info.Name)
				         .ThenBy(m => m.Info.GetParameters().Length))
			{
				var methodInfo = (MethodInfo)method.Info;
				var heading = methodInfo.Name + methodInfo.ToParametersString();
				heading = _documentMethodDetails ? _writer.HeadingLink(heading, _writer.Bold(heading)) : _writer.Bold(heading);
				_writer.WriteTableRow(
					heading,
					methodInfo.ToTypeNameString(_typeLinkConverter, true),
					ProcessTags(method.Comments.Summary));
			}
		}

		if (allFields.Count > 0)
		{
			_writer.WriteH2("Fields");
			_writer.WriteTableTitle("Name", "Type", "Summary");
			foreach (var field in allFields)
				_writer.WriteTableRow(
					_writer.Bold(field.Info.Name),
					field.Info.ToTypeNameString(_typeLinkConverter, true),
					ProcessTags(field.Comments.Summary));
		}

		if (_documentMethodDetails)
		{
			if (allConstructors.Count > 0)
			{
				_writer.WriteH2("Constructors");
				foreach (var (info, comments) in allConstructors
					         .OrderBy(m => m.Info.GetParameters().Length))
					WriteMethodDetails(typeData.Type.ToNameString(), info, comments);
			}

			if (allMethods.Count > 0)
			{
				_writer.WriteH2("Methods");
				foreach (var (info, comments) in allMethods
					         .OrderBy(m => m.Info.Name)
					         .ThenBy(m => m.Info.GetParameters().Length))
					WriteMethodDetails(info.Name, info, comments);
			}
		}
	}

	private void WriteMethodDetails(string name, MethodBase info, MethodComments comments)
	{
		_writer.WriteH3(name + info.ToParametersString());
		_writer.WriteLine(ProcessTags(comments.Summary));
		if (comments.Parameters.Count > 0)
		{
			var parameters = info.GetParameters();
			var i = 0;
			_writer.WriteTableTitle("Parameter", "Type", "Description");
			foreach (var (paramName, text) in comments.Parameters)
				_writer.WriteTableRow(paramName,
					parameters[i++].ToTypeNameString(_typeLinkConverter, true),
					ProcessTags(text));
		}

		_writer.WriteLine("");

		if (info is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void))
		{
			_writer.WriteH3("Returns");
			_writer.WriteLine(methodInfo.ToTypeNameString(_typeLinkConverter, true));
			_writer.WriteLine(ProcessTags(comments.Returns));
		}

		WriteExample(comments.Example);
	}

	private async Task WriteDocumentationForType(TypeCollection.TypeInformation typeData)
	{
		if (typeData.Type.IsEnum)
		{
			await WriteEnumDocumentation(typeData.Type);
			return;
		}

		await WriteClassDocumentation(typeData);
	}

	public void WriteExample(string? example)
	{
		if (example.IsNullOrEmpty()) return;

		_writer.WriteH2("Examples");
		_writer.WriteLine(ProcessTags(example!));
	}

	public void WriteRemarks(string? remarks)
	{
		if (remarks.IsNullOrEmpty()) return;

		_writer.WriteH2("Remarks");
		_writer.WriteLine(ProcessTags(remarks!));
	}
}