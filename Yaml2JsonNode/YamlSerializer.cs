using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Yaml2JsonNode;

/// <summary>
/// Provides common serializer functionality.
/// </summary>
public static class YamlSerializer
{
	/// <summary>
	/// Serializes an object to a YAML string
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="obj">The object.</param>
	/// <param name="options">(optional) JSON serializer options.</param>
	/// <param name="configure">(optional) YAML serializer configuration method.</param>
	/// <returns>The YAML string.</returns>
	public static string Serialize<T>(T obj, JsonSerializerOptions? options = null, Action<SerializerBuilder>? configure = null)
	{
		var json = JsonSerializer.SerializeToNode(obj, options);
		var yaml = json!.ToYamlNode();

		return Serialize(yaml, configure);
	}

	/// <summary>
	/// Serializes a YAML stream (a collection of documents) to a string.
	/// </summary>
	/// <param name="yaml">The stream.</param>
	/// <param name="configure">(optional) YAML serializer configuration method.</param>
	/// <returns>The YAML string.</returns>
	public static string Serialize(YamlStream yaml, Action<SerializerBuilder>? configure = null)
	{
		var builder = new SerializerBuilder();

		configure?.Invoke(builder);

		var serializer = builder.Build();

		using var writer = new StringWriter();

		foreach (var document in yaml.Documents)
		{
			serializer.Serialize(writer, document.RootNode);
		}

		return writer.ToString();
	}

	/// <summary>
	/// Serializes a YAML document to a string.
	/// </summary>
	/// <param name="yaml">The document.</param>
	/// <param name="configure">(optional) YAML serializer configuration method.</param>
	/// <returns>The YAML string.</returns>
	public static string Serialize(YamlDocument yaml, Action<SerializerBuilder>? configure = null)
	{
		return Serialize(new YamlStream(yaml), configure);
	}

	/// <summary>
	/// Serializes a YAML node to a string
	/// </summary>
	/// <param name="yaml">The node.</param>
	/// <param name="configure">(optional) YAML serializer configuration method.</param>
	/// <returns>The YAML string.</returns>
	public static string Serialize(YamlNode yaml, Action<SerializerBuilder>? configure = null)
	{
		return Serialize(new YamlDocument(yaml), configure);
	}

	/// <summary>
	/// Deserializes the first YAML document found in text to an object.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="yamlText">The YAML text.</param>
	/// <param name="options"></param>
	/// <returns>The object or null.</returns>
	public static T? Deserialize<T>(string yamlText, JsonSerializerOptions? options = null)
	{
		var yaml = Parse(yamlText).First();
		var json = yaml.ToJsonNode();

		return json.Deserialize<T>(options);
	}

	/// <summary>
	/// Parses YAML text into a stream (collection of documents).
	/// </summary>
	/// <param name="yamlText">The YAML text.</param>
	/// <returns>A YAML stream representing the content of the text.</returns>
	public static YamlStream Parse(string yamlText)
	{
		using var reader = new StringReader(yamlText);
		var yamlStream = new YamlStream();
		yamlStream.Load(reader);
		return yamlStream;
	}
}