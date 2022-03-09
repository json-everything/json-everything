using System.Text.Json;

namespace Json.More;

/// <summary>
/// Acts as an intermediary that allows an "implicit casting"-like behavior between
/// native JSON types and <see cref="JsonElement"/>.
/// </summary>
public readonly struct JsonElementProxy
{
	private readonly JsonElement _value;

	private JsonElementProxy(JsonElement value)
	{
		_value = value;
	}

	/// <summary>
	/// Converts an `int` to a <see cref="JsonElementProxy"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator JsonElementProxy(int value) => new(value.AsJsonElement());
	/// <summary>
	/// Converts an `long` to a <see cref="JsonElementProxy"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator JsonElementProxy(long value) => new(value.AsJsonElement());
	/// <summary>
	/// Converts an `short` to a <see cref="JsonElementProxy"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator JsonElementProxy(short value) => new(value.AsJsonElement());
	/// <summary>
	/// Converts an `float` to a <see cref="JsonElementProxy"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator JsonElementProxy(float value) => new(value.AsJsonElement());
	/// <summary>
	/// Converts an `double` to a <see cref="JsonElementProxy"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator JsonElementProxy(double value) => new(value.AsJsonElement());
	/// <summary>
	/// Converts an `decimal` to a <see cref="JsonElementProxy"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator JsonElementProxy(decimal value) => new(value.AsJsonElement());
	/// <summary>
	/// Converts an `string` to a <see cref="JsonElementProxy"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator JsonElementProxy(string value) => new(value.AsJsonElement());
	/// <summary>
	/// Converts an `bool` to a <see cref="JsonElementProxy"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator JsonElementProxy(bool value) => new(value.AsJsonElement());

	/// <summary>
	/// Converts a <see cref="JsonElementProxy"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="proxy">The proxy.</param>
	public static implicit operator JsonElement(JsonElementProxy proxy) => proxy._value;
}