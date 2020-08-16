using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cysharp.Text;

namespace Json.Pointer
{
	[JsonConverter(typeof(JsonPointerJsonConverter))]
	public struct JsonPointer
	{
		public static readonly JsonPointer Empty =
			new JsonPointer
				{
					_source = string.Empty,
					Segments = new PointerSegment[0]
			};
		public static readonly JsonPointer UrlEmpty =
			new JsonPointer
				{
					_source = "#",
					IsUriEncoded = true,
					Segments = new PointerSegment[0]
				};

		private string _source;

		public string Source => _source ??= _BuildSource();
		public PointerSegment[] Segments { get; private set; }
		public bool IsUriEncoded { get; private set; }

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

		public static JsonPointer Create(IEnumerable<PointerSegment> segments, bool isUriEncoded)
		{
			return new JsonPointer
				{
					Segments = segments.ToArray(),
					IsUriEncoded = isUriEncoded
				};
		}

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

		public override string ToString()
		{
			return Source;
		}
	}
}
