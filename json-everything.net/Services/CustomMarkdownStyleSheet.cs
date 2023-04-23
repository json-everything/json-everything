using ColorCode;
using ColorCode.Styling;
using ColorCode.Styling.StyleSheets;

namespace JsonEverythingNet.Services;

public class CustomMarkdownStyleSheet : IStyleSheet
{
	private static readonly Color _dullRed = new(163, 21, 21);
	private static readonly StyleDictionary _styles;

	static CustomMarkdownStyleSheet()
	{
		var styleDictionary = new StyleDictionary
		{
			new("Plain Text")
			{
				Foreground = Color.Black,
				Background = Color.White,
				CssClassName = "plainText"
			},
			new("HTML Server-Side Script")
			{
				Background = Color.Yellow,
				CssClassName = "htmlServerSideScript"
			},
			new("HTML Comment")
			{
				Foreground = Color.Green,
				CssClassName = "htmlComment"
			},
			new("Html Tag Delimiter")
			{
				Foreground = Color.Blue,
				CssClassName = "htmlTagDelimiter"
			},
			new("HTML Element ScopeName")
			{
				Foreground = _dullRed,
				CssClassName = "htmlElementName"
			},
			new("HTML Attribute ScopeName")
			{
				Foreground = Color.Red,
				CssClassName = "htmlAttributeName"
			},
			new("HTML Attribute Value")
			{
				Foreground = Color.Blue,
				CssClassName = "htmlAttributeValue"
			},
			new("HTML Operator")
			{
				Foreground = Color.Blue,
				CssClassName = "htmlOperator"
			},
			new("Comment")
			{
				Foreground = Color.Green,
				CssClassName = "comment"
			},
			new("XML Doc Tag")
			{
				Foreground = Color.Gray,
				CssClassName = "xmlDocTag"
			},
			new("XML Doc Comment")
			{
				Foreground = Color.Green,
				CssClassName = "xmlDocComment"
			},
			new("String")
			{
				Foreground = new Color(214, 157, 133),
				CssClassName = "string"
			},
			new("String (C# @ Verbatim)")
			{
				Foreground = new Color(214, 157, 133),
				CssClassName = "stringCSharpVerbatim"
			},
			new("Keyword")
			{
				Foreground = new Color(86, 156, 214),
				CssClassName = "keyword"
			},
			new("Preprocessor Keyword")
			{
				Foreground = new Color(86, 156, 214),
				CssClassName = "preprocessorKeyword"
			},
			new("HTML Entity")
			{
				Foreground = Color.Red,
				CssClassName = "htmlEntity"
			},
			new("XML Attribute")
			{
				Foreground = Color.Red,
				CssClassName = "xmlAttribute"
			},
			new("XML Attribute Quotes")
			{
				Foreground = Color.Black,
				CssClassName = "xmlAttributeQuotes"
			},
			new("XML Attribute Value")
			{
				Foreground = Color.Blue,
				CssClassName = "xmlAttributeValue"
			},
			new("XML CData Section")
			{
				Foreground = Color.Gray,
				CssClassName = "xmlCDataSection"
			},
			new("XML Comment")
			{
				Foreground = Color.Green,
				CssClassName = "xmlComment"
			},
			new("XML Delimiter")
			{
				Foreground = Color.Blue,
				CssClassName = "xmlDelimiter"
			},
			new("XML Name")
			{
				Foreground = _dullRed,
				CssClassName = "xmlName"
			},
			new("Class Name")
			{
				Foreground = Color.MediumTurquoise,
				CssClassName = "className"
			},
			new("CSS Selector")
			{
				Foreground = _dullRed,
				CssClassName = "cssSelector"
			},
			new("CSS Property Name")
			{
				Foreground = Color.Red,
				CssClassName = "cssPropertyName"
			},
			new("CSS Property Value")
			{
				Foreground = Color.Blue,
				CssClassName = "cssPropertyValue"
			},
			new("SQL System Function")
			{
				Foreground = Color.Magenta,
				CssClassName = "sqlSystemFunction"
			},
			new("PowerShell PowerShellAttribute")
			{
				Foreground = Color.PowderBlue,
				CssClassName = "powershellAttribute"
			},
			new("PowerShell Operator")
			{
				Foreground = Color.Gray,
				CssClassName = "powershellOperator"
			},
			new("PowerShell Type")
			{
				Foreground = Color.Teal,
				CssClassName = "powershellType"
			},
			new("PowerShell Variable")
			{
				Foreground = Color.OrangeRed,
				CssClassName = "powershellVariable"
			},
			new("Type")
			{
				Foreground = Color.Teal,
				CssClassName = "type"
			},
			new("Type Variable")
			{
				Foreground = Color.Teal,
				Italic = true,
				CssClassName = "typeVariable"
			},
			new("Name Space")
			{
				Foreground = Color.Navy,
				CssClassName = "namespace"
			},
			new("Constructor")
			{
				Foreground = Color.Purple,
				CssClassName = "constructor"
			},
			new("Predefined")
			{
				Foreground = Color.Navy,
				CssClassName = "predefined"
			},
			new("Pseudo Keyword")
			{
				Foreground = Color.Navy,
				CssClassName = "pseudoKeyword"
			},
			new("String Escape")
			{
				Foreground = Color.Gray,
				CssClassName = "stringEscape"
			},
			new("Control Keyword")
			{
				Foreground = Color.Blue,
				CssClassName = "controlKeyword"
			},
			new("Number")
			{
				CssClassName = "number"
			},
			new("Operator")
			{
				CssClassName = "operator"
			},
			new("Delimiter")
			{
				CssClassName = "delimiter"
			},
			new("Markdown Header")
			{
				Bold = true,
				CssClassName = "markdownHeader"
			},
			new("Markdown Code")
			{
				Foreground = Color.Teal,
				CssClassName = "markdownCode"
			},
			new("Markdown List Item")
			{
				Bold = true,
				CssClassName = "markdownListItem"
			},
			new("Markdown Emphasized")
			{
				Italic = true,
				CssClassName = "italic"
			},
			new("Markdown Bold")
			{
				Bold = true,
				CssClassName = "bold"
			}
		};
		_styles = styleDictionary;
	}

	public string Name => nameof(CustomMarkdownStyleSheet);

	public StyleDictionary Styles => _styles;
}

public static class StyleSheets
{
	public static readonly IStyleSheet Default = new DefaultStyleSheet();
	public static readonly IStyleSheet Custom = new CustomMarkdownStyleSheet();
}