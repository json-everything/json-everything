using System.Text.Json.Serialization;

namespace JsonEverythingNet.Models;

[JsonConverter(typeof(JsonStringEnumConverter<BubbleSize>))]
public enum BubbleSize
{
	None,
	Small = 25,
	Medium = 50,
	Large = 100
}