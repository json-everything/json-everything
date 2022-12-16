using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Yaml2JsonNode.Tests;

public static class Extensions
{
	// Source: https://github.com/aaubry/YamlDotNet/issues/131#issuecomment-628280069
	public static string SerializeToJson(this YamlNode node)
	{
		var stream = new YamlStream { new(node) };
		using var writer = new StringWriter();
		stream.Save(writer);

		using var reader = new StringReader(writer.ToString());
		var deserializer = new Deserializer();
		var yamlObject = deserializer.Deserialize(reader);
		var serializer = new SerializerBuilder()
			.JsonCompatible()
			.Build();
		return serializer.Serialize(yamlObject).Trim();
	}
}