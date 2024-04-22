using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Web;
using Json.More;

namespace Json.Pointer;

/// <summary>
/// Represents a JSON Pointer IAW RFC 6901.
/// </summary>
[JsonConverter(typeof(JsonPointerJsonConverter))]
[TypeConverter(typeof(JsonPointerTypeConverter))]
public struct JsonPointer : IEquatable<JsonPointer>
{
	/// <summary>
	/// The empty pointer.
	/// </summary>
	public static readonly JsonPointer Empty = new();

	private string _plain;

	public Range[] Segments { get; }
	public readonly  ReadOnlySpan<char> this[int index] => _plain.AsSpan()[Segments[index]];

	public JsonPointer()
	{
		_plain = string.Empty;
		Segments = [];
	}
	private JsonPointer(ReadOnlySpan<Range> segments)
	{
		Segments = [..segments];
	}

	/// <summary>
	/// Parses a JSON Pointer from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <returns>A JSON Pointer.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="PointerParseException"><paramref name="source"/> does not contain a valid pointer or contains a pointer of the wrong kind.</exception>
	public static JsonPointer Parse(ReadOnlySpan<char> source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (source.Length == 0) return Empty;

		var i = 0;
		if (source[i] == '#')
		{
			source = HttpUtility.UrlDecode(source.ToString()).AsSpan();  // allocation
			i++;
		}

		if (source.Length == i) return Empty;
		if (source[i] != '/')
			throw new PointerParseException("Pointer must start with either `#` or `/` or be empty");

		i++;
		var count = 0;
		var start = i;
		using var owner = MemoryPool<Range>.Shared.Rent();
		var span = owner.Memory.Span;
		while (i < source.Length)
		{
			if (source[i] == '/')
			{
				span[count] = new Range(start, i);
				start = i + 1;
				count++;
			}

			_ = ConsiderEscapes(source, ref i);

			i++;
		}

		span[count] = start >= source.Length
			? new Range(0, 0)
			: new Range(start, i);

		return new JsonPointer(span[..(count+1)])
		{
			_plain = source.ToString()  // allocation
		};
	}

	/// <summary>
	/// Parses a JSON Pointer from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="pointer">The resulting pointer.</param>
	/// <returns>`true` if the parse was successful; `false` otherwise.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static bool TryParse(ReadOnlySpan<char> source, out JsonPointer pointer)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (source.Length == 0)
		{
			pointer = Empty;
			return true;
		}

		var i = 0;
		if (source[i] == '#')
		{
			source = HttpUtility.UrlDecode(source.ToString()).AsSpan();  // allocation
			i++;
		}

		if (source.Length == i)
		{
			pointer = Empty;
			return true;
		}

		if (source[i] != '/')
		{
			pointer = Empty;
			return false;
		}

		i++;
		var count = 0;
		var start = i;
		using var owner = MemoryPool<Range>.Shared.Rent();
		var span = owner.Memory.Span;
		while (i < source.Length)
		{
			if (source[i] == '/')
			{
				span[count] = new Range(start, i);
				start = i + 1;
				count++;
			}

			if (!TryConsiderEscapes(source, ref i))
			{
				pointer = Empty;
				return false;
			}

			i++;
		}

		span[count] = start >= source.Length
			? new Range(0, 0)
			: new Range(start, i);

		pointer = new JsonPointer(span[..(count + 1)])
		{
			_plain = source.ToString()  // allocation
		};
		return true;
	}

	/// <summary>
	/// Creates a new JSON Pointer from a collection of segments.
	/// </summary>
	/// <param name="segments">A collection of segments.</param>
	/// <returns>The JSON Pointer.</returns>
	/// <remarks>This method creates un-encoded pointers only.</remarks>
	public static JsonPointer Create(params PointerSegment[] segments)
	{
		return Create((IEnumerable<PointerSegment>)segments);
	}

	/// <summary>
	/// Creates a new JSON Pointer from a collection of segments.
	/// </summary>
	/// <param name="segments">A collection of segments.</param>
	/// <returns>The JSON Pointer.</returns>
	public static JsonPointer Create(IEnumerable<PointerSegment> segments)
	{
		using var owner = MemoryPool<char>.Shared.Rent();
		var span = owner.Memory.Span;

		var i = 0;
		foreach (var segment in segments)
		{
			span[i] = '/';
			i++;
			foreach (var ch in segment.Value)
			{
				span[i] = ch;
				i++;
			}
		}

		return Parse(owner.Memory.Span[..i]);
	}

	/// <summary>
	/// Generates a JSON Pointer from a lambda expression.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="expression">The lambda expression which gives the pointer path.</param>
	/// <param name="options">(optional) Options for creating the pointer.</param>
	/// <returns>The JSON Pointer.</returns>
	/// <exception cref="NotSupportedException">
	/// Thrown when the lambda expression contains a node that is not a property access or
	/// <see cref="int"/>-valued indexer.
	/// </exception>
	public static JsonPointer Create<T>(Expression<Func<T, object>> expression, PointerCreationOptions? options = null)
	{
		PointerSegment GetSegment(MemberInfo member)
		{
			var attribute = member.GetCustomAttribute<JsonPropertyNameAttribute>();
			if (attribute is not null)
				return attribute.Name;

			return options.PropertyNameResolver(member);
		}

		// adapted from https://stackoverflow.com/a/2616980/878701
		object GetValue(Expression? member)
		{
			if (member == null) return "null";

			var objectMember = Expression.Convert(member, typeof(object));
			var getterLambda = Expression.Lambda<Func<object>>(objectMember);
			var getter = getterLambda.Compile();
			return getter();
		}

		options ??= PointerCreationOptions.Default;

		var body = expression.Body;
		using var owner = MemoryPool<PointerSegment>.Shared.Rent();
		var segments = owner.Memory.Span;
		var i = segments.Length - 1;
		while (body != null)
		{
			if (body.NodeType == ExpressionType.Convert && body is UnaryExpression unary)
				body = unary.Operand;

			if (body is MemberExpression me)
			{
				segments[i] = GetSegment(me.Member);
				body = me.Expression;
			}
			else if (body is MethodCallExpression mce1 &&
					 mce1.Method.Name.StartsWith("get_") &&
					 mce1.Arguments.Count == 1 &&
					 mce1.Arguments[0].Type == typeof(int))
			{
				var arg = mce1.Arguments[0];
				var value = GetValue(arg) ?? throw new NotSupportedException("Method in expression must return a non-null expression");
				segments[i] = value.ToString()!;
				body = mce1.Object;
			}
			else if (body is MethodCallExpression { Method: { IsStatic: true, Name: nameof(Enumerable.Last) } } mce2 &&
			         mce2.Method.DeclaringType == typeof(Enumerable))
			{
				segments[i] = "-";
				body = mce2.Arguments[0];
			}
			else if (body is BinaryExpression { Right: ConstantExpression arrayIndexExpression } binaryExpression
					 and { NodeType: ExpressionType.ArrayIndex })
			{
				// Array index
				segments[i] = arrayIndexExpression.Value!.ToString()!;
				body = binaryExpression.Left;
			}
			else if (body is ParameterExpression) break; // this is the param of the expression itself.
			else throw new NotSupportedException($"Expression nodes of type {body.NodeType} are not currently supported.");

			i--;
		}

		i++;

		return Create(segments[i..].ToArray());
	}

	/// <summary>
	/// Concatenates a pointer onto the current pointer.
	/// </summary>
	/// <param name="other">Another pointer.</param>
	/// <returns>A new pointer.</returns>
	public readonly JsonPointer Combine(JsonPointer other)
	{
		using var owner = MemoryPool<char>.Shared.Rent();
		var span = owner.Memory.Span;
		_plain.AsSpan().CopyTo(span);
		span[_plain.Length] = '/';
		var nextSegment = span[(_plain.Length + 1)..];
		other._plain.AsSpan().CopyTo(nextSegment);

		return Parse(span);
	}

	/// <summary>
	/// Concatenates additional segments onto the current pointer.
	/// </summary>
	/// <param name="additionalSegments">The additional segments.</param>
	/// <returns>A new pointer.</returns>
	public readonly JsonPointer Combine(params PointerSegment[] additionalSegments)
	{
		using var owner = MemoryPool<char>.Shared.Rent();
		var span = owner.Memory.Span;
		_plain.AsSpan().CopyTo(span);

		var i = _plain.Length;
		foreach (var segment in additionalSegments)
		{
			span[i] = '/';
			var nextSegment = span[(i + 1)..];
			segment.Value.AsSpan().CopyTo(nextSegment);
			i += segment.Value.Length;
		}

		return Parse(span);
	}

	/// <summary>
	/// Evaluates the pointer over a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="root">The <see cref="JsonElement"/>.</param>
	/// <returns>The sub-element at the pointer's location, or null if the path does not exist.</returns>
	public readonly JsonElement? Evaluate(JsonElement root)
	{
		var current = root;
		var kind = root.ValueKind;

		var span = _plain.AsSpan();
		foreach (var segment in Segments)
		{
			ReadOnlySpan<char> segmentValue;
			switch (kind)
			{
				case JsonValueKind.Array:
					segmentValue = span[segment];
					if (segmentValue.Length == 0) return null;
					if (segmentValue.Length == 1 && segmentValue[0] == '0')
					{
						if (current.GetArrayLength() == 0) return null;
						current = current.EnumerateArray().First();
						break;
					}
					if (segmentValue[0] == '0') return null;
					if (segmentValue.Length == 1 && segmentValue[0] == '-') return current.EnumerateArray().LastOrDefault();
					if (!segmentValue.TryGetInt(out var index)) return null;
					if (index >= current.GetArrayLength()) return null;
					if (index < 0) return null;

					current = current.EnumerateArray().ElementAt(index);
					break;
				case JsonValueKind.Object:
					segmentValue = span[segment];
					var found = false;
					foreach (var p in current.EnumerateObject())
					{
						if (!SegmentEquals(segmentValue, p.Name)) continue;

						current = p.Value;
						found = true;
						break;
					}
					if (!found) return null;
					break;
				default:
					return null;
			}
			kind = current.ValueKind;
		}

		return current;
	}

	/// <summary>
	/// Evaluates the pointer over a <see cref="JsonNode"/>.
	/// </summary>
	/// <param name="root">The <see cref="JsonNode"/>.</param>
	/// <param name="result">The result, if return value is true; null otherwise</param>
	/// <returns>true if a value exists at the indicate path; false otherwise.</returns>
	public readonly bool TryEvaluate(JsonNode? root, out JsonNode? result)
	{
		var current = root;
		result = null;

		var span = _plain.AsSpan();
		foreach (var segment in Segments)
		{
			ReadOnlySpan<char> segmentValue;
			switch (current)
			{
				case JsonArray array:
					segmentValue = span[segment];
					if (segmentValue.Length == 0) return false;
					if (segmentValue.Length is 1 && segmentValue[0] == '0')
					{
						if (array.Count == 0) return false;
						current = current[0];
						break;
					}
					if (segmentValue[0] == '0') return false;
					if (segmentValue.Length is 1 && segmentValue[0] == '-')
					{
						result = array.Last();
						return true;
					}
					if (!segmentValue.TryGetInt(out var index)) return false;
					if (index >= array.Count) return false;
					if (index < 0) return false;
					current = array[index];
					break;
				case JsonObject obj:
					segmentValue = span[segment];
					var found = false;
					foreach (var kvp in obj)
					{
						if (!SegmentEquals(segmentValue, kvp.Key)) continue;
						
						current = kvp.Value;
						found = true;
						break;
					}

					if (!found) return false;
					break;
				default:
					return false;
			}
		}

		result = current;
		return true;
	}

	/// <summary>Returns the string representation of this instance.</summary>
	/// <param name="pointerStyle">Indicates whether to URL-encode the pointer.</param>
	/// <returns>The string representation.</returns>
	public string ToString(JsonPointerStyle pointerStyle)
	{
		return _plain;

		//string BuildString(StringBuilder sb)
		//{
		//	foreach (var segment in OldSegments)
		//	{
		//		sb.Append('/');
		//		sb.Append(segment.ToString(pointerStyle));
		//	}

		//	return sb.ToString();
		//}

		//return pointerStyle != JsonPointerStyle.UriEncoded
		//	? _plain
		//	: _uriEncoded ??= BuildString(new StringBuilder("#"));
	}

	/// <summary>Returns the string representation of this instance.</summary>
	/// <returns>The string representation.</returns>
	public override string ToString()
	{
		return ToString(JsonPointerStyle.Plain);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public readonly bool Equals(JsonPointer other)
	{
		return string.Equals(_plain, other._plain, StringComparison.Ordinal);
	}

	/// <summary>Indicates whether this instance and a specified object are equal.</summary>
	/// <param name="obj">The object to compare with the current instance.</param>
	/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		if (obj is not JsonPointer pointer) return false;

		return Equals(pointer);
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
	public readonly override int GetHashCode()
	{
		// ReSharper disable once NonReadonlyMemberInGetHashCode
		return _plain.GetHashCode();
	}

	/// <summary>
	/// Compares a pointer segment to an expected string.
	/// </summary>
	/// <param name="segment">The pointer segment.</param>
	/// <param name="expected">The expected string.</param>
	/// <returns>true if the segment and the string are equivalent; false otherwise.</returns>
	/// <remarks>
	/// The escapes for `~` and `/` are considered for the pointer segment, but not the expected string.
	/// </remarks>
	public static bool SegmentEquals(ReadOnlySpan<char> segment, string expected)
	{
		if (string.IsNullOrEmpty(expected)) return segment.Length == 0;
		if (segment.Length == 0) return false;

		var aIndex = 0;
		var bIndex = 0;

		while (aIndex < segment.Length && bIndex < expected.Length)
		{
			var aChar = ConsiderEscapes(segment, ref aIndex);
			var bChar = expected[bIndex];

			if (aChar != bChar) return false;

			aIndex++;
			bIndex++;
		}

		return true;
	}

	private static char ConsiderEscapes(ReadOnlySpan<char> value, ref int index)
	{
		var ch = value[index];
		if (ch == '~')
		{
			if (index + 1 >= value.Length)
				throw new PointerParseException("Value does not represent a valid JSON Pointer segment");
			ch = value[index + 1] switch
			{
				'0' => '~',
				'1' => '/',
				_ => throw new PointerParseException("Value does not represent a valid JSON Pointer segment")
			};
			index++;
		}

		return ch;
	}

	private static bool TryConsiderEscapes(ReadOnlySpan<char> value, ref int index)
	{
		var ch = value[index];
		if (ch == '~')
		{
			if (index + 1 >= value.Length) return false;
			return value[index + 1] is '0' or '1';
		}

		return true;
	}

	/// <summary>
	/// Evaluates equality via <see cref="Equals(JsonPointer)"/>.
	/// </summary>
	/// <param name="left">A JSON Pointer.</param>
	/// <param name="right">A JSON Pointer.</param>
	/// <returns>`true` if the pointers are equal; `false` otherwise.</returns>
	public static bool operator ==(JsonPointer? left, JsonPointer? right)
	{
		return Equals(left, right);
	}

	/// <summary>
	/// Evaluates inequality via <see cref="Equals(JsonPointer)"/>.
	/// </summary>
	/// <param name="left">A JSON Pointer.</param>
	/// <param name="right">A JSON Pointer.</param>
	/// <returns>`false` if the pointers are equal; `true` otherwise.</returns>
	public static bool operator !=(JsonPointer? left, JsonPointer? right)
	{
		return !Equals(left, right);
	}
}
