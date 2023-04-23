using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Issue435()
	{
		var backslashes = "C:\\Folder\\Issue435_schema.json";
		var backslashesUri = new Uri(backslashes);
		
		var fileUri = new Uri($"file:///{backslashes}", UriKind.RelativeOrAbsolute);
		var pointer = new Uri("#/$defs/DerivedType", UriKind.RelativeOrAbsolute);

		var backslashesUriResult = new Uri(backslashesUri, pointer);
		var fileUriResult = new Uri(fileUri, pointer);

		Console.WriteLine("File path: {0}", backslashes);
		Console.WriteLine("File path (URI): {0}", backslashesUri.OriginalString);
		Console.WriteLine();
		Console.WriteLine("Direct URI: {0}", fileUri.OriginalString);
		Console.WriteLine();
		Console.WriteLine("Fragment: {0}", pointer.OriginalString);
		Console.WriteLine();
		Console.WriteLine("Combined, backslashes: {0}", backslashesUriResult);
		Console.WriteLine("Combined, direct:      {0}", fileUriResult);
	}
}