using System.Text.Json.Serialization;

namespace JsonEverythingNet.Models;

[JsonConverter(typeof(JsonStringEnumConverter<BubbleSize>))]
public enum BubbleSize
{
	None,
	Small,
	Medium,
	Large
}