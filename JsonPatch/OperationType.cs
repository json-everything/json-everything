using System.ComponentModel;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Patch
{
	/// <summary>
	/// Enumerates the valid JSON Patch operation types.
	/// </summary>
	[JsonConverter(typeof(EnumStringConverter<OperationType>))]
	public enum OperationType
	{
		/// <summary>
		/// Default value.  Not valid.
		/// </summary>
		Unknown,
		/// <summary>
		/// Represents the `add` operation.
		/// </summary>
		[Description("add")]
		Add,
		/// <summary>
		/// Represents the `remove` operation.
		/// </summary>
		[Description("remove")]
		Remove,
		/// <summary>
		/// Represents the `replace` operation.
		/// </summary>
		[Description("replace")]
		Replace,
		/// <summary>
		/// Represents the `move` operation.
		/// </summary>
		[Description("move")]
		Move,
		/// <summary>
		/// Represents the `copy` operation.
		/// </summary>
		[Description("copy")]
		Copy,
		/// <summary>
		/// Represents the `test` operation.
		/// </summary>
		[Description("test")]
		Test
	}
}