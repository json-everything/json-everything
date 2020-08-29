using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cysharp.Text;

namespace Json.Pointer
{
	/// <summary>
	/// Represents a JSON Pointer IAW RFC 6901.
	/// </summary>
	[JsonConverter(typeof(JsonPointerJsonConverter))]
	public struct JsonPointer : IEquatable<JsonPointer>
	{
		/// <summary>
		/// The empty pointer.
		/// </summary>
		public static readonly JsonPointer Empty =
			new JsonPointer
				{
					_source = string.Empty,
					Segments = new PointerSegment[0]
			};
		/// <summary>
		/// The empty pointer in URL-style.
		/// </summary>
		public static readonly JsonPointer UrlEmpty =
			new JsonPointer
				{
					_source = "#",
					IsUriEncoded = true,
					Segments = new PointerSegment[0]
				};

		private string _source;

		/// <summary>
		/// Gets the source string for the pointer.
		/// </summary>
		public string Source => _source ??= _BuildSource();
		/// <summary>
		/// Gets the collection of pointer segments.
		/// </summary>
		public PointerSegment[] Segments { get; private set; }
		/// <summary>
		/// Gets whether the pointer is URL-encoded.
		/// </summary>
		public bool IsUriEncoded { get; private set; }

		/// <summary>
		/// Parses a JSON Pointer from a string.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <returns>A JSON Pointer.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
		/// <exception cref="PointerParseException"><paramref name="source"/> does not contain a valid pointer.</exception>
		public static JsonPointer Parse(string source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (source == string.Empty) return Empty;
			if (source == "#") return UrlEmpty;

			bool isUriEncoded;
			var parts = source.Split('/');
			var i = 0;
			if (parts[0] == "#" || parts[0] == string.Empty)
			{
				isUriEncoded = parts[0] == "#";
				i++;
			}
			else throw new PointerParseException("Pointer must start with either `#` or `/` or be empty");

			var segments = new PointerSegment[parts.Length - i];
			for (; i < parts.Length; i++)
			{
				segments[i - 1] = PointerSegment.Parse(parts[i], isUriEncoded);
			}

			return new JsonPointer
				{
					_source = source,
					Segments = segments,
					IsUriEncoded = isUriEncoded
				};
		}

		/// <summary>
		/// Parses a JSON Pointer from a string.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="pointer">The resulting pointer.</param>
		/// <returns><code>true</code> if the parse was successful; <code>false</code> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
		public static bool TryParse(string source, out JsonPointer pointer)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (source == string.Empty)
			{
				pointer = Empty;
				return true;
			}
			if (source == "#")
			{
				pointer = UrlEmpty;
				return true;
			}

			bool isUriEncoded;
			var parts = source.Split('/');
			var i = 0;
			if (parts[0] == "#" || parts[0] == string.Empty)
			{
				isUriEncoded = parts[0] == "#";
				i++;
			}
			else
			{
				pointer = default;
				return false;
			}

			var segments = new PointerSegment[parts.Length - i];
			for (; i < parts.Length; i++)
			{
				var part = parts[i];
				if (!PointerSegment.TryParse(part, isUriEncoded, out var segment))
				{
					pointer = default;
					return false;
				}

				segments[i - 1] = segment;
			}

			pointer = new JsonPointer
				{
					_source = source,
					Segments = segments,
					IsUriEncoded = isUriEncoded
				};
			return true;
		}

		/// <summary>
		/// Creates a new JSON Pointer from a collection of segments.
		/// </summary>
		/// <param name="segments">A collection of segments.</param>
		/// <param name="isUriEncoded">Whether the pointer should be URL-encoded.</param>
		/// <returns>The JSON Pointer.</returns>
		public static JsonPointer Create(IEnumerable<PointerSegment> segments, bool isUriEncoded)
		{
			return new JsonPointer
				{
					Segments = segments.ToArray(),
					IsUriEncoded = isUriEncoded
				};
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
					Segments = segments,
					IsUriEncoded = IsUriEncoded
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
					Segments = segments,
					IsUriEncoded = IsUriEncoded
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

		private string _BuildSource()
		{
			var builder = ZString.CreateStringBuilder();
			if (IsUriEncoded)
				builder.Append('#');

			if (Segments != null)
			{
				foreach (var segment in Segments)
				{
					builder.Append('/');
					builder.Append(segment.Source);
				}
			}

			return builder.ToString();
		}

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>The fully qualified type name.</returns>
		public override string ToString()
		{
			return Source;
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(JsonPointer other)
		{
			return Segments.SequenceEqual(other.Segments) && IsUriEncoded == other.IsUriEncoded;
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is JsonPointer other && Equals(other);
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (Segments.GetCollectionHashCode() * 397) ^ IsUriEncoded.GetHashCode();
			}
		}

		/// <summary>
		/// Evaluates equality via <see cref="Equals(JsonPointer)"/>.
		/// </summary>
		/// <param name="left">A JSON Pointer.</param>
		/// <param name="right">A JSON Pointer.</param>
		/// <returns><code>true</code> if the pointers are equal; <code>false</code> otherwise.</returns>
		public static bool operator ==(JsonPointer left, JsonPointer right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Evaluates inequality via <see cref="Equals(JsonPointer)"/>.
		/// </summary>
		/// <param name="left">A JSON Pointer.</param>
		/// <param name="right">A JSON Pointer.</param>
		/// <returns><code>false</code> if the pointers are equal; <code>true</code> otherwise.</returns>
		public static bool operator !=(JsonPointer left, JsonPointer right)
		{
			return !left.Equals(right);
		}
	}
}
