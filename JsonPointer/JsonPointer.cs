using System;
using System.Buffers;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Web;

namespace Json.Pointer;

/// <summary>
/// Represents a JSON Pointer IAW RFC 6901.
/// </summary>
[JsonConverter(typeof(JsonPointerJsonConverter))]
[TypeConverter(typeof(JsonPointerTypeConverter))]
public readonly struct JsonPointer : IEquatable<JsonPointer>
{
	/// <summary>
	/// The empty pointer.
	/// </summary>
	public static readonly JsonPointer Empty = new();

	/// <summary>
	/// Defines the default segment array size.  Used during parsing.  Default is 50.  Increase as needed if you expect pointers with many segments.
	/// </summary>
	// ReSharper disable once FieldCanBeMadeReadOnly.Global
	// ReSharper disable once ConvertToConstant.Global
	public static int DefaultMaxSize = 50;

	private readonly string _plain;
	private readonly Range[] _segments;

	/// <summary>
	/// Gets the number of segments in the pointer.
	/// </summary>
	public int SegmentCount => _segments.Length;
	/// <summary>
	/// Gets a segment value by index.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>The indicated segment value as a span.</returns>
	public ReadOnlySpan<char> this[Index index] => _plain.AsSpan()[_segments[index]];


	/// <summary>
	/// Creates the empty pointer.
	/// </summary>
	public JsonPointer()
	{
		_plain = string.Empty;
		_segments = [];
	}
	private JsonPointer(string plain, ReadOnlySpan<Range> segments)
	{
		_plain = plain;
		_segments = [..segments];
	}

	/// <summary>
	/// Parses a JSON Pointer from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <returns>A JSON Pointer.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="PointerParseException"><paramref name="source"/> does not contain a valid pointer or contains a pointer of the wrong kind.</exception>
	public static JsonPointer Parse(string source)
	{
		if (source.Length == 0) return Empty;

		if (source[0] == '#')
			source = HttpUtility.UrlDecode(source[1..]);  // allocation

		if (source.Length == 0) return Empty;
		if (source[0] != '/')
			throw new PointerParseException("Pointer must start with either `#/` or `/` or be empty");

		var i = 1;
		var count = 0;
		var start = 1;
		var sourceSpan = source.AsSpan();
		Span<Range> span = stackalloc Range[DefaultMaxSize];
		while (i < source.Length)
		{
			if (source[i] == '/')
			{
				span[count] = new Range(start, i);
				start = i + 1;
				count++;
			}

			_ = sourceSpan.Decode(ref i);

			i++;
		}

		span[count] = start >= source.Length
			? new Range(0, 0)
			: new Range(start, i);

		return new JsonPointer(source, span[..(count + 1)]);
	}

	/// <summary>
	/// Parses a JSON Pointer from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="pointer">The resulting pointer.</param>
	/// <returns>`true` if the parse was successful; `false` otherwise.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static bool TryParse(string source, out JsonPointer pointer)
	{
		if (source.Length == 0)
		{
			pointer = Empty;
			return true;
		}

		if (source[0] == '#')
		{
			source = HttpUtility.UrlDecode(source[1..]);  // allocation
		}

		if (source.Length == 0)
		{
			pointer = Empty;
			return true;
		}

		if (source[0] != '/')
		{
			pointer = Empty;
			return false;
		}

		var i = 1;
		var count = 0;
		var start = 1;
		var sourceSpan = source.AsSpan();
		Span<Range> span = stackalloc Range[DefaultMaxSize];
		while (i < source.Length)
		{
			if (source[i] == '/')
			{
				span[count] = new Range(start, i);
				start = i + 1;
				count++;
			}

			if (!sourceSpan.TryDecode(ref i))
			{
				pointer = Empty;
				return false;
			}

			i++;
		}

		span[count] = start >= source.Length
			? new Range(0, 0)
			: new Range(start, i);

		pointer = new JsonPointer(source, span[..(count + 1)]);
		return true;
	}

	/// <summary>
	/// Creates a single-segment pointer.
	/// </summary>
	/// <param name="segment"></param>
	/// <returns></returns>
	public static JsonPointer Create(PointerSegment segment)
	{
		Span<char> span = stackalloc char[segment.GetLength() * 2 + 1];

		var i = 0;
		span[i] = '/';
		i++;
		foreach (var ch in segment.GetValue().Encode())
		{
			span[i] = ch;
			i++;
		}

		return new JsonPointer(span[..i].ToString(), [(1..i)]);
	}

	/// <summary>
	/// Creates a new JSON Pointer from a collection of segments.
	/// </summary>
	/// <param name="segments">A collection of segments.</param>
	/// <returns>The JSON Pointer.</returns>
	/// <remarks>This method creates un-encoded pointers only.</remarks>
	public static JsonPointer Create(params PointerSegment[] segments)
	{
		Span<char> str = stackalloc char[GetPointerLength(segments)];
		Span<Range> ranges = stackalloc Range[segments.Length];

		var i = 0;
		var s = 0;
		foreach (var segment in segments)
		{
			str[i] = '/';
			i++;
			var start = i;
			foreach (var ch in segment.GetValue().Encode())
			{
				str[i] = ch;
				i++;
			}

			ranges[s] = start..i;
			s++;
		}

		return new JsonPointer(str[..i].ToString(), ranges[..s]);
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
	public JsonPointer Combine(JsonPointer other)
	{
		if (other._segments.Length == 0) return this;
		if (_segments.Length == 0) return other;

		Span<char> span = stackalloc char[_plain.Length + other._plain.Length];
		Span<Range> ranges = stackalloc Range[_segments.Length + other._segments.Length];
		_plain.AsSpan().CopyTo(span);
		_segments.CopyTo(ranges);
		other._plain.AsSpan().CopyTo(span[_plain.Length..]);

		for (int i = 0; i < other._segments.Length; i++)
		{
			var current = other._segments[i];
			var offset = (current.Start.Value + _plain.Length)..(current.End.Value + _plain.Length);
			ranges[_segments.Length + i] = offset;
		}

		return new JsonPointer(span.ToString(), ranges);
	}

	/// <summary>
	/// Concatenates additional segments onto the current pointer.
	/// </summary>
	/// <param name="additionalSegments">The additional segments.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer Combine(params PointerSegment[] additionalSegments)
	{
		if (additionalSegments.Length == 0) return this;
		if (_segments.Length == 0) return Create(additionalSegments);

		Span<char> span = stackalloc char[_plain.Length + GetPointerLength(additionalSegments)];
		Span<Range> ranges = stackalloc Range[_segments.Length + additionalSegments.Length];
		_plain.AsSpan().CopyTo(span);
		_segments.CopyTo(ranges);

		var i = _plain.Length;
		var s = _segments.Length;
		foreach (var segment in additionalSegments)
		{
			span[i] = '/';
			i++;
			var start = i;
			var nextSegment = span[i..];
			var value = segment.GetValue().Encode();
			value.CopyTo(nextSegment);
			i += value.Length;
			ranges[s] = start..i;
			s++;
		}

		return new JsonPointer(span[..i].ToString(), ranges[..s]);
	}

	private static int GetPointerLength(PointerSegment[] segments)
	{
		var sum = 1;
		foreach (var segment in segments)
		{
			sum += segment.GetLength() * 2;
		}

		return sum;
	}

	/// <summary>
	/// Creates a new pointer that retrieves an ancestor (left side, start) of the represented location.
	/// </summary>
	/// <param name="levels">How many levels to go back.  Default is 1, which gets the immediate parent.</param>
	/// <returns>A new pointer.</returns>
	/// <exception cref="ArgumentException">
	/// Thrown if <paramref name="levels"/> is less than zero or more than the number of segments.
	/// </exception>
	public JsonPointer GetAncestor(int levels = 1)
	{
		if (levels == 0) return this;
		if (levels < 0 || levels > _segments.Length)
			throw new IndexOutOfRangeException("Ancestor cannot be reached");
		if (levels == _segments.Length) return Empty;

		var end = _segments[^(levels+1)].End;
		return Parse(_plain.AsSpan()[..end].ToString());
	}

	/// <summary>
	/// Creates a new pointer that retrieves the local part (right side, end) of the represented location.
	/// </summary>
	/// <param name="skip">How many levels to skip from the start of the pointer.</param>
	/// <returns>A new pointer.</returns>
	/// <exception cref="ArgumentException">
	/// Thrown if <paramref name="skip"/> is less than zero or more than the number of segments.
	/// </exception>
	public JsonPointer GetLocal(int skip)
	{
		if (skip == 0) return this;
		if (skip == _segments.Length) return Empty;
		if (skip < 0 || skip > _segments.Length)
			throw new IndexOutOfRangeException("Local cannot be reached");

		var start = _segments[skip].Start.Value - 1; // capture the slash
		return Parse(_plain.AsSpan()[start..].ToString());
	}

	/// <summary>
	/// Evaluates the pointer over a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="root">The <see cref="JsonElement"/>.</param>
	/// <returns>The sub-element at the pointer's location, or null if the path does not exist.</returns>
	public JsonElement? Evaluate(JsonElement root)
	{
		var current = root;
		var kind = root.ValueKind;

		var span = _plain.AsSpan();
		foreach (var segment in _segments)
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
						if (!segmentValue.SegmentEquals(p.Name)) continue;

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
	public bool TryEvaluate(JsonNode? root, out JsonNode? result)
	{
		var current = root;
		result = null;

		var span = _plain.AsSpan();
		foreach (var segment in _segments)
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
						if (!segmentValue.SegmentEquals(kvp.Key)) continue;
						
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
		return pointerStyle == JsonPointerStyle.UriEncoded
			? HttpUtility.HtmlEncode(_plain)
			: _plain;
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
	public bool Equals(JsonPointer other)
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
	public override int GetHashCode()
	{
		// ReSharper disable once NonReadonlyMemberInGetHashCode
		return _plain.GetHashCode();
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
