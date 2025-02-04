using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Pointer;

/// <summary>
/// Represents a JSON Pointer IAW RFC 6901.
/// </summary>
[JsonConverter(typeof(JsonPointerJsonConverter))]
[TypeConverter(typeof(JsonPointerTypeConverter))]
public class JsonPointer : IEquatable<JsonPointer>, IReadOnlyList<string>
{
	/// <summary>
	/// The empty pointer.
	/// </summary>
	public static readonly JsonPointer Empty = new();

	private readonly string[] _decodedSegments;
	private string? _plain;
	private int? _hashCode;

	/// <summary>
	/// Gets the number of segments in the pointer.
	/// </summary>
	public int Count => _decodedSegments.Length;
	
	/// <summary>
	/// Gets a segment value by index.
	/// </summary>
	/// <param name="i">The index.</param>
	/// <returns>The indicated segment value as a span.</returns>
	public string this[int i] => _decodedSegments[i];

	// There's a problem with how PolySharp adds the Range type that means I can't
	// expose this in the netstandard2.0 build.  When the lib is consumed by an app
	// running anything previous to .Net 8, the netstandard2.0 version is used.
	// If the generated Range type is exposed, it conflicts with the built-in Range
	// type, and this feature can't be used anyway.  I'd like to have a solution
	// for this, but I haven't been able to think of one.
#if !NETSTANDARD2_0
	/// <summary>
	/// Creates a new pointer with the indicated segments.
	/// </summary>
	/// <param name="r">The segment range for the new pointer.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer this[Range r] => GetSubPointer(r);
#endif

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)_decodedSegments).GetEnumerator();

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private JsonPointer()
	{
		_decodedSegments = [];
	}

	private JsonPointer(ReadOnlySpan<string> segments, string? plain = null)
	{
		_decodedSegments = [..segments];
		_plain = plain;
	}

	private JsonPointer(string[] segments, string? plain = null)
	{
		_decodedSegments = segments;
		_plain = plain;
	}

	/// <summary>
	/// Parses a JSON Pointer from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <returns>A JSON Pointer.</returns>
	/// <exception cref="PointerParseException"><paramref name="source"/> does not contain a valid pointer or contains a pointer of the wrong kind.</exception>
	public static JsonPointer Parse(ReadOnlySpan<char> source)
	{
		return ParseCore(source, null);
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
		return ParseCore(source.AsSpan(), source);
	}

	private static JsonPointer ParseCore(ReadOnlySpan<char> source, string? plain)
	{
		if (source.Length == 0) return Empty;

		if (source[0] == '#')
		{
#if NET9_0_OR_GREATER
			source = Uri.UnescapeDataString(source[1..]);
#elif NET8_0_OR_GREATER
			source = Uri.UnescapeDataString(new(source[1..]));
#else
			source = Uri.UnescapeDataString(source[1..].ToString()).AsSpan();
#endif

			if (source.Length == 0) return Empty;

			// We cannot use the original input, because ToString() always returns the non-URI form.
			// So we force lazy evaluation in ToString() to not pay the cost here.
			plain = null; 
		}

		if (source[0] != '/')
			throw new PointerParseException("Pointer must start with either `#/` or `/` or be empty");

		if (source.Length == 1)
			return new JsonPointer([""], "/");

		if (TryParseInternal(source, plain, out var result))
			return result;

		throw new PointerParseException($"Value '{source.ToString()}' does not represent a valid JSON Pointer");
	}

	private static int CountSegments(ReadOnlySpan<char> source)
	{
		var size = 0;
		var pos = source.IndexOf('/');

		while (pos >= 0)
		{
			size++;

			source = source[(pos + 1)..];
			pos = source.IndexOf('/');
		}

		return size;
	}

#if NET9_0_OR_GREATER

	private static bool TryParseInternal(ReadOnlySpan<char> source, string? plain, [NotNullWhen(true)] out JsonPointer? result)
	{
		var segmentCount = CountSegments(source);
		var segments = new string[segmentCount];
		var segmentIndex = 0;

		source = source[1..];
		foreach (var segmentRange in source.Split('/'))
		{
			if (source[segmentRange].TryDecodeSegment(out var segment))
			{
				segments[segmentIndex++] = segment;
			}
			else
			{
				result = null;
				return false;
			}
		}

		result = new JsonPointer(segments, plain);
		return true;
	}

#else

	private static bool TryParseInternal(ReadOnlySpan<char> source, string? plain, [NotNullWhen(true)] out JsonPointer? result)
	{
		var segmentCount = CountSegments(source);
		var segments = new string[segmentCount];

		source = source[1..];
		var segmentIndex = 0;

		var sourceIndex = source.IndexOf('/');
		while (sourceIndex >= 0)
		{
			if (!source[..sourceIndex].TryDecodeSegment(out var segment))
			{
				result = null;
				return false;
			}

			segments[segmentIndex++] = segment;
			
			source = source[(sourceIndex + 1)..];
			sourceIndex = source.IndexOf('/');
		}

		if (!source.TryDecodeSegment(out var segment2))
		{
			result = null;
			return false;
		}

		segments[segmentIndex] = segment2;

		result = new JsonPointer(segments, plain);
		return true;
	}

#endif

	/// <summary>
	/// Parses a JSON Pointer from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="pointer">The resulting pointer.</param>
	/// <returns>`true` if the parse was successful; `false` otherwise.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static bool TryParse(ReadOnlySpan<char> source, [NotNullWhen(true)] out JsonPointer? pointer)
	{
		return TryParseCore(source, null, out pointer);
	}

	private static bool TryParseCore(ReadOnlySpan<char> source, string? plain, [NotNullWhen(true)] out JsonPointer? pointer)
	{
		if (source.Length == 0)
		{
			pointer = Empty;
			return true;
		}

		if (source[0] == '#')
		{
#if NET9_0_OR_GREATER
			source = Uri.UnescapeDataString(source[1..]);
#elif NET8_0_OR_GREATER
			source = Uri.UnescapeDataString(new(source[1..]));
#else
			source = Uri.UnescapeDataString(source[1..].ToString()).AsSpan();
#endif

			if (source.Length == 0)
			{
				pointer = Empty;
				return true;
			}

			// We cannot use the original input, because ToString() always returns the non-URI form.
			// So we force lazy evaluation in ToString() to not pay the cost here.
			plain = null;
		}

		if (source[0] != '/')
		{
			pointer = null;
			return false;
		}

		if (source.Length == 1)
		{
			pointer = new JsonPointer([""], "/");
			return true;
		}

		return TryParseInternal(source, plain, out pointer);
	}

	/// <summary>
	/// Parses a JSON Pointer from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="pointer">The resulting pointer.</param>
	/// <returns>`true` if the parse was successful; `false` otherwise.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static bool TryParse(string source, [NotNullWhen(true)] out JsonPointer? pointer)
	{
		return TryParse(source.AsSpan(), out pointer);
	}

	/// <summary>
	/// Creates a new JSON Pointer from a collection of segments.
	/// </summary>
	/// <param name="segments">A collection of segments.</param>
	/// <returns>The JSON Pointer.</returns>
	/// <remarks>This method creates un-encoded pointers only.</remarks>
	public static JsonPointer Create(params PointerSegment[] segments)
	{
#if NET9_0_OR_GREATER
		return Create(segments.AsSpan());
#else
		var array = new string[segments.Length];

		for (var i = 0; i < segments.Length; i++)
		{
			array[i] = segments[i].Value;
		}

		return new JsonPointer(array);
#endif
	}

#if NET9_0_OR_GREATER

	/// <summary>
	/// Creates a new JSON Pointer from a collection of segments.
	/// </summary>
	/// <param name="segments">A collection of segments.</param>
	/// <returns>The JSON Pointer.</returns>
	/// <remarks>This method creates un-encoded pointers only.</remarks>
	public static JsonPointer Create(params ReadOnlySpan<PointerSegment> segments)
	{
		var array = new string[segments.Length];
		
		for (var i = 0; i < segments.Length; i++)
		{
			array[i] = segments[i].Value;
		}

		return new JsonPointer(array);
	}

#endif

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
		string GetSegment(MemberInfo member)
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

#if NET9_0_OR_GREATER
		return Create(segments[i..]);
#else
		return Create(segments[i..].ToArray());
#endif
	}

	/// <summary>
	/// Concatenates a pointer onto the current pointer.
	/// </summary>
	/// <param name="other">Another pointer.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer Combine(JsonPointer other)
	{
		if (other._decodedSegments.Length == 0) return this;
		if (_decodedSegments.Length == 0) return other;

		var array = new string[_decodedSegments.Length + other._decodedSegments.Length];
		Array.Copy(_decodedSegments, array, _decodedSegments.Length);
		Array.Copy(other._decodedSegments, 0, array, _decodedSegments.Length, other._decodedSegments.Length);

		return new JsonPointer(array);
	}

	/// <summary>
	/// Concatenates additional segments onto the current pointer.
	/// </summary>
	/// <param name="additionalSegments">The additional segments.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer Combine(params PointerSegment[] additionalSegments)
	{
#if NET9_0_OR_GREATER
		return Combine(additionalSegments.AsSpan());
#else
		if (additionalSegments.Length == 0) return this;
		if (_decodedSegments.Length == 0) return Create(additionalSegments);

		var array = new string[_decodedSegments.Length + additionalSegments.Length];
		Array.Copy(_decodedSegments, array, _decodedSegments.Length);

		for (int i = 0; i < additionalSegments.Length; i++)
		{
			array[_decodedSegments.Length + i] = additionalSegments[i].Value;
		}

		return new JsonPointer(array);
#endif
	}

#if NET9_0_OR_GREATER

	/// <summary>
	/// Concatenates additional segments onto the current pointer.
	/// </summary>
	/// <param name="additionalSegments">The additional segments.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer Combine(params ReadOnlySpan<PointerSegment> additionalSegments)
	{
		if (additionalSegments.Length == 0) return this;
		if (_decodedSegments.Length == 0) return Create(additionalSegments);

		var array = new string[_decodedSegments.Length + additionalSegments.Length];
		Array.Copy(_decodedSegments, array, _decodedSegments.Length);

		for (int i = 0; i < additionalSegments.Length; i++)
		{
			array[_decodedSegments.Length + i] = additionalSegments[i].Value;
		}

		return new JsonPointer(array);
	}

#endif

	/// <summary>
	/// Creates a new pointer retaining the starting segments.
	/// </summary>
	/// <param name="levels">How many levels to remove from the end of the pointer.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer GetAncestor(int levels)
	{
		return new(_decodedSegments.AsSpan()[..^levels]);
	}
	/// <summary>
	/// Creates a new pointer retaining the ending segments.
	/// </summary>
	/// <param name="levels">How many levels to keep from the end of the pointer.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer GetLocal(int levels)
	{
		return new(_decodedSegments.AsSpan()[levels..]);
	}

#if !NETSTANDARD2_0
	/// <summary>
	/// Creates a new pointer with the indicated segments.
	/// </summary>
	/// <param name="range">The segment range for the new pointer.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer GetSubPointer(Range range)
	{
		return new(_decodedSegments.AsSpan()[range]);
	}
#endif

	/// <summary>
	/// Evaluates the pointer over a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="root">The <see cref="JsonElement"/>.</param>
	/// <returns>The sub-element at the pointer's location, or null if the path does not exist.</returns>
	public JsonElement? Evaluate(JsonElement root)
	{
		var current = root;
		var kind = root.ValueKind;

		foreach (var segment in _decodedSegments)
		{
			switch (kind)
			{
				case JsonValueKind.Array:
					if (segment.Length == 0) return null;
					if (segment is ['0'])
					{
						if (current.GetArrayLength() == 0) return null;
						current = current.EnumerateArray().First();
						break;
					}
					if (segment[0] == '0') return null;
					if (segment is ['-']) return current.EnumerateArray().LastOrDefault();
					if (!int.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)) return null;
					if (index >= current.GetArrayLength()) return null;
					if (index < 0) return null;

					current = current.EnumerateArray().ElementAt(index);
					break;
				case JsonValueKind.Object:
					var found = false;
					foreach (var p in current.EnumerateObject())
					{
						if (segment != p.Name) continue;

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

		foreach (var segment in _decodedSegments)
		{
			switch (current)
			{
				case JsonArray array:
					if (segment.Length == 0) return false;
					if (segment is ['0'])
					{
						if (array.Count == 0) return false;
						current = current[0];
						break;
					}
					if (segment[0] == '0') return false;
					if (segment is ['-'])
					{
						result = array.Last();
						return true;
					}
					if (!int.TryParse(segment, out var index)) return false;
					if (index >= array.Count) return false;
					if (index < 0) return false;
					current = array[index];
					break;
				case JsonObject obj:
					if (!obj.TryGetValue(segment, out current, out _)) return false;
					break;
				default:
					return false;
			}
		}

		result = current;
		return true;
	}

	/// <summary>Returns the string representation of this instance.</summary>
	/// <returns>The string representation.</returns>
	public override string ToString()
	{
		if (_plain is not null)
			return _plain;

		if (_decodedSegments.Length == 0)
			return "";

		var max = 0;
		var total = 0;

		foreach (var segment in _decodedSegments)
		{
			max = Math.Max(max, segment.Length);
			total += segment.Length;
		}

		return _plain ??= (total < 1024
			? ToStringSmall(total)
			: ToStringLarge(total));
	}

	private string ToStringSmall(int total)
	{
		Span<char> final = stackalloc char[total * 2 + _decodedSegments.Length];

		var length = 0;
		foreach (var segment in _decodedSegments)
		{
			final[length] = '/';
			length++;
			var localLength = segment.AsSpan().Encode(final.Slice(length, segment.Length * 2));
			length += localLength;
		}

		return final[..length].ToString();
	}

	private string ToStringLarge(int total)
	{
		using var memory = MemoryPool<char>.Shared.Rent(total * 2 + _decodedSegments.Length);
		Span<char> final = memory.Memory.Span;

		var length = 0;
		foreach (var segment in _decodedSegments)
		{
			final[length] = '/';
			length++;
			var localLength = segment.AsSpan().Encode(final.Slice(length, segment.Length * 2));
			length += localLength;
		}

		return final[..length].ToString();
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(JsonPointer? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;

		return _decodedSegments.SequenceEqual(other._decodedSegments);
	}

	/// <summary>Indicates whether this instance and a specified object are equal.</summary>
	/// <param name="obj">The object to compare with the current instance.</param>
	/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as JsonPointer);
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
	public override int GetHashCode()
	{
		// ReSharper disable once NonReadonlyMemberInGetHashCode
		return _hashCode ??= _decodedSegments.GetCollectionHashCode();
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
