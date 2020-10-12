using System.ComponentModel;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Patch
{
	[JsonConverter(typeof(EnumStringConverter<OperationType>))]
	public enum OperationType
	{
		Unknown,
		[Description("add")]
		Add,
		[Description("remove")]
		Remove,
		[Description("replace")]
		Replace,
		[Description("move")]
		Move,
		[Description("copy")]
		Copy,
		[Description("test")]
		Test
	}
}