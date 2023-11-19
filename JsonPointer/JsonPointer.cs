using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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
public class JsonPointer : IEquatable<JsonPointer>
{
	/// <summary>
	/// The empty pointer.
	/// </summary>
	public static readonly JsonPointer Empty =
		new()
		{
			Segments = []
		};

	private string? _uriEncoded;
	private string? _plain;

	/// <summary>
	/// Gets the collection of pointer segments.
	/// </summary>
	public PointerSegment[] Segments { get; private set; } = null!;

	private JsonPointer() { }

	/// <summary>
	/// Parses a JSON Pointer from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <returns>A JSON Pointer.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="PointerParseException"><paramref name="source"/> does not contain a valid pointer or contains a pointer of the wrong kind.</exception>
	public static JsonPointer Parse(string source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (source == string.Empty) return Empty;

		if (source[0] == '#')
			source = HttpUtility.UrlDecode(source);

		var parts = source.Split('/');
		var i = 0;
		if (parts[0] == "#" || parts[0] == string.Empty)
		{
			i++;
		}
		else throw new PointerParseException("Pointer must start with either `#` or `/` or be empty");

		var segments = new PointerSegment[parts.Length - i];
		for (; i < parts.Length; i++)
		{
			segments[i - 1] = PointerSegment.Parse(parts[i]);
		}

		return new JsonPointer
		{
			Segments = segments
		};
	}

	/// <summary>
	/// Parses a JSON Pointer from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="pointer">The resulting pointer.</param>
	/// <returns>`true` if the parse was successful; `false` otherwise.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static bool TryParse(string source, out JsonPointer? pointer)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (source == string.Empty)
		{
			pointer = Empty;
			return true;
		}

		if (source[0] == '#')
			source = HttpUtility.UrlDecode(source);


		var parts = source.Split('/');
		var i = 0;
		if (parts[0] == "#" || parts[0] == string.Empty)
		{
			i++;
		}
		else
		{
			pointer = null;
			return false;
		}

		var segments = new PointerSegment[parts.Length - i];
		for (; i < parts.Length; i++)
		{
			var part = parts[i];
			if (!PointerSegment.TryParse(part, out var segment))
			{
				pointer = null;
				return false;
			}

			segments[i - 1] = segment!;
		}

		pointer = new JsonPointer
		{
			Segments = segments
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
		return new JsonPointer
		{
			Segments = segments
		};
	}

	/// <summary>
	/// Creates a new JSON Pointer from a collection of segments.
	/// </summary>
	/// <param name="segments">A collection of segments.</param>
	/// <returns>The JSON Pointer.</returns>
	public static JsonPointer Create(IEnumerable<PointerSegment> segments)
	{
		return new JsonPointer
		{
			Segments = segments.ToArray()
		};
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

			return options!.PropertyNameResolver!(member);
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
		var segments = new List<PointerSegment>();
		while (body != null)
		{
			if (body.NodeType == ExpressionType.Convert && body is UnaryExpression unary)
				body = unary.Operand;

			if (body is MemberExpression me)
			{
				segments.Insert(0, GetSegment(me.Member));
				body = me.Expression;
			}
			else if (body is MethodCallExpression mce1 &&
					 mce1.Method.Name.StartsWith("get_") &&
					 mce1.Arguments.Count == 1 &&
					 mce1.Arguments[0].Type == typeof(int))
			{
				var arg = mce1.Arguments[0];
				var value = GetValue(arg);
				segments.Insert(0, PointerSegment.Create(value.ToString()));
				body = mce1.Object;
			}
			else if (body is MethodCallExpression { Method: { IsStatic: true, Name: nameof(Enumerable.Last) } } mce2 &&
			         mce2.Method.DeclaringType == typeof(Enumerable))
			{
				segments.Insert(0, PointerSegment.Create("-"));
				body = mce2.Arguments[0];
			}
			else if (body is BinaryExpression { Right: ConstantExpression arrayIndexExpression } binaryExpression
					 and { NodeType: ExpressionType.ArrayIndex })
			{
				// Array index
				segments.Insert(0, PointerSegment.Create(arrayIndexExpression.Value!.ToString()!));
				body = binaryExpression.Left;
			}
			else if (body is ParameterExpression) break; // this is the param of the expression itself.
			else throw new NotSupportedException($"Expression nodes of type {body.NodeType} are not currently supported.");
		}

		return Create(segments);
	}

	/// <summary>
	/// Concatenates a pointer onto the current pointer.
	/// </summary>
	/// <param name="other">Another pointer.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer Combine(JsonPointer other)
	{
		var segments = new PointerSegment[Segments.Length + other.Segments.Length];
		Segments.CopyTo(segments, 0);
		other.Segments.CopyTo(segments, Segments.Length);

		return new JsonPointer
		{
			Segments = segments
		};
	}

	/// <summary>
	/// Concatenates additional segments onto the current pointer.
	/// </summary>
	/// <param name="additionalSegments">The additional segments.</param>
	/// <returns>A new pointer.</returns>
	public JsonPointer Combine(params PointerSegment[] additionalSegments)
	{
		var segments = new PointerSegment[Segments.Length + additionalSegments.Length];
		Segments.CopyTo(segments, 0);
		additionalSegments.CopyTo(segments, Segments.Length);

		return new JsonPointer
		{
			Segments = segments
		};
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

		foreach (var segment in Segments)
		{
			string segmentValue;
			switch (kind)
			{
				case JsonValueKind.Array:
					segmentValue = segment.Value;
					if (segmentValue == "0")
					{
						if (current.GetArrayLength() == 0) return null;
						current = current.EnumerateArray().First();
						break;
					}
					if (segmentValue[0] == '0') return null;
					if (segmentValue == "-") return current.EnumerateArray().LastOrDefault();
					if (!int.TryParse(segmentValue, out var index)) return null;
					if (index >= current.GetArrayLength()) return null;
					if (index < 0) return null;
					current = current.EnumerateArray().ElementAt(index);
					break;
				case JsonValueKind.Object:
					segmentValue = segment.Value;
					var found = false;
					foreach (var p in current.EnumerateObject())
					{
						if (p.NameEquals(segmentValue))
						{
							current = p.Value;
							found = true;
							break;
						}
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

		foreach (var segment in Segments)
		{
			string segmentValue;
			switch (current)
			{
				case JsonArray array:
					segmentValue = segment.Value;
					if (segmentValue == string.Empty) return false;
					if (segmentValue == "0")
					{
						if (array.Count == 0) return false;
						current = current[0];
						break;
					}
					if (segmentValue[0] == '0') return false;
					if (segmentValue == "-")
					{
						result = array.Last();
						return true;
					}
					if (!int.TryParse(segmentValue, out var index)) return false;
					if (index >= array.Count) return false;
					if (index < 0) return false;
					current = array[index];
					break;
				case JsonObject obj:
					segmentValue = segment.Value;
					if (!obj.TryGetValue(segmentValue, out var found, out _)) return false;
					current = found;
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
		string BuildString(StringBuilder sb)
		{
			foreach (var segment in Segments)
			{
				sb.Append('/');
				sb.Append(segment.ToString(pointerStyle));
			}

			return sb.ToString();
		}

		return pointerStyle != JsonPointerStyle.UriEncoded
			? _plain ??= BuildString(new StringBuilder())
			: _uriEncoded ??= BuildString(new StringBuilder("#"));
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
	public bool Equals(JsonPointer? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;

		return Segments.SequenceEqual(other.Segments);
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
		return Segments.GetCollectionHashCode();
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