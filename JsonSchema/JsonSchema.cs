using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.More;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Represents a JSON Schema.
	/// </summary>
	[JsonConverter(typeof(SchemaJsonConverter))]
	public class JsonSchema : IRefResolvable, IEquatable<JsonSchema>
	{
		/// <summary>
		/// The empty schema <code>{}</code>.  Functionally equivalent to <see cref="True"/>.
		/// </summary>
		public static readonly JsonSchema Empty = new JsonSchema(Enumerable.Empty<IJsonSchemaKeyword>(), null);
		/// <summary>
		/// The <code>true</code> schema.  Passes all instances.
		/// </summary>
		public static readonly JsonSchema True = new JsonSchema(true);
		/// <summary>
		/// The <code>false</code> schema.  Fails all instances.
		/// </summary>
		public static readonly JsonSchema False = new JsonSchema(false);

		/// <summary>
		/// Gets the keywords contained in the schema.
		/// </summary>
		public IReadOnlyCollection<IJsonSchemaKeyword> Keywords { get; }
		/// <summary>
		/// Gets other non-keyword (or unknown keyword) properties in the schema.
		/// </summary>
		public IReadOnlyDictionary<string, JsonElement> OtherData { get; }

		internal bool? BoolValue { get; }

		private JsonSchema(bool value)
		{
			BoolValue = value;
		}
		internal JsonSchema(IEnumerable<IJsonSchemaKeyword> keywords, IReadOnlyDictionary<string, JsonElement> otherData)
		{
			Keywords = keywords.ToArray();
			OtherData = otherData;
		}

		/// <summary>
		/// Loads text from a file and deserializes a <see cref="JsonSchema"/>.
		/// </summary>
		/// <param name="fileName">The filename to load.</param>
		/// <returns>A new <see cref="JsonSchema"/>.</returns>
		/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
		public static JsonSchema FromFile(string fileName)
		{
			var text = File.ReadAllText(fileName);
			return FromText(text);
		}

		/// <summary>
		/// Deserializes a <see cref="JsonSchema"/> from text.
		/// </summary>
		/// <param name="jsonText">The text to parse.</param>
		/// <returns>A new <see cref="JsonSchema"/>.</returns>
		/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
		public static JsonSchema FromText(string jsonText)
		{
			return JsonSerializer.Deserialize<JsonSchema>(jsonText);
		}

		/// <summary>
		/// Deserializes a <see cref="JsonSchema"/> from a stream.
		/// </summary>
		/// <param name="source">A stream.</param>
		/// <returns>A new <see cref="JsonSchema"/>.</returns>
		public static ValueTask<JsonSchema> FromStream(Stream source)
		{
			return JsonSerializer.DeserializeAsync<JsonSchema>(source);
		}

		/// <summary>
		/// Validates an instance against this schema.
		/// </summary>
		/// <param name="root">The root instance.</param>
		/// <param name="options">The options to use for this validation.</param>
		/// <returns>A <see cref="ValidationResults"/> that provides the outcome of the validation.</returns>
		public ValidationResults Validate(JsonElement root, ValidationOptions options = null)
		{
			options ??= ValidationOptions.Default;

			var context = new ValidationContext
				{
					Options = options ?? ValidationOptions.Default,
					LocalInstance = root,
					InstanceLocation = JsonPointer.UrlEmpty,
					InstanceRoot = root,
					SchemaLocation = JsonPointer.UrlEmpty,
					SchemaRoot = this
				};

			RegisterSubschemas(context.Options.SchemaRegistry, null);
			ValidateSubschema(context);

			var results = new ValidationResults(context);
			switch (options.OutputFormat)
			{
				case OutputFormat.Flag:
					results.ToFlag();
					break;
				case OutputFormat.Basic:
					results.ToBasic();
					break;
				case OutputFormat.Detailed:
					results.ToDetailed();
					break;
				case OutputFormat.Verbose:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return results;
		}

		/// <summary>
		/// Registers a subschema.  To be called from <see cref="IRefResolvable"/> keywords.
		/// </summary>
		/// <param name="registry">The registry into which the subschema should be registered.</param>
		/// <param name="currentUri">The current URI.</param>
		public void RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			if (Keywords == null) return; // boolean cases

			var idKeyword = Keywords.OfType<IdKeyword>().SingleOrDefault();
			if (idKeyword != null)
			{
				currentUri = idKeyword.UpdateUri(currentUri);
				var parts = idKeyword.Id.OriginalString.Split(new[] {'#'}, StringSplitOptions.None);
				var fragment = parts.Length > 1 ? parts[1] : null;
				if (string.IsNullOrEmpty(fragment) || fragment[0] == '/')
					registry.Register(currentUri, this);
				else
					registry.RegisterAnchor(currentUri, fragment, this);
			}

			var anchorKeyword = Keywords.OfType<AnchorKeyword>().SingleOrDefault();
			if (anchorKeyword != null) 
				registry.RegisterAnchor(currentUri, anchorKeyword.Anchor, this);

			var keywords = Keywords.OfType<IRefResolvable>().OrderBy(k => ((IJsonSchemaKeyword)k).Priority());
			foreach (var keyword in keywords)
			{
				keyword.RegisterSubschemas(registry, currentUri);
			}
		}

		/// <summary>
		/// Validates as a subschema.  To be called from within keywords.
		/// </summary>
		/// <param name="context">The validation context for this validation pass.</param>
		public void ValidateSubschema(ValidationContext context)
		{
			if (BoolValue.HasValue)
			{
				context.IsValid = BoolValue.Value;
				context.SchemaLocation = context.SchemaLocation.Combine(PointerSegment.Create($"${BoolValue}".ToLowerInvariant()));
				if (!context.IsValid)
					context.Message = "All values fail against the false schema";
				return;
			}

			var metaSchemaUri = Keywords.OfType<SchemaKeyword>().FirstOrDefault()?.Schema;
			var keywords = context.Options.FilterKeywords(Keywords, metaSchemaUri, context.Options.SchemaRegistry);

			ValidationContext newContext = null;
			var overallResult = true;
			foreach (var keyword in keywords.OrderBy(k => k.Priority()))
			{
				var previousContext = newContext;
				newContext = ValidationContext.From(context,
					subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create(keyword.Keyword())));
				newContext.CurrentAnchor ??= previousContext?.CurrentAnchor;
				newContext.ParentContext = context;
				newContext.LocalSchema = this;
				newContext.ImportAnnotations(previousContext);
				if (context.HasNestedContexts)
					newContext.SiblingContexts.AddRange(context.NestedContexts);
				newContext.RequiredInResult = keyword.IsApplicator();
				keyword.Validate(newContext);
				overallResult &= newContext.IsValid;
				if (!overallResult && context.ApplyOptimizations) break;
				if (!newContext.Ignore)
					context.NestedContexts.Add(newContext);
			}

			context.IsValid = overallResult;
			if (context.IsValid)
				context.ImportAnnotations(newContext);
		}

		internal (JsonSchema, Uri) FindSubschema(JsonPointer pointer, Uri currentUri)
		{
			IRefResolvable resolvable = this;
			for (var i = 0; i < pointer.Segments.Length; i++)
			{
				var segment = pointer.Segments[i];
				var newResolvable = resolvable.ResolvePointerSegment(segment.Value);
				if (newResolvable == null)
				{
					// TODO: document that this process does not consider `$id` in extraneous data
					if (resolvable is JsonSchema subSchema &&
					    subSchema.OtherData != null &&
					    subSchema.OtherData.TryGetValue(segment.Value, out var element))
					{
						var newPointer = JsonPointer.Create(pointer.Segments.Skip(i + 1), true);
						var value = newPointer.Evaluate(element);
						var asSchema = FromText(value.ToString());
						return (asSchema, currentUri);
					}

					return (null, currentUri);
				}

				if (newResolvable is JsonSchema schema && schema.Keywords != null)
				{
					var idKeyword = schema.Keywords.OfType<IdKeyword>().SingleOrDefault();
					if (idKeyword != null && i != pointer.Segments.Length - 1)
						currentUri = idKeyword.UpdateUri(currentUri);
				}

				resolvable = newResolvable;
			}

			if (!(resolvable is JsonSchema))
				resolvable = resolvable.ResolvePointerSegment(null);

			return (resolvable as JsonSchema, currentUri);
		}

		IRefResolvable IRefResolvable.ResolvePointerSegment(string value)
		{
			var keyword = Keywords.FirstOrDefault(k => k.Keyword() == value);
			return keyword as IRefResolvable;
		}

		/// <summary>
		/// Implicitly converts a boolean value into one of the boolean schemas.
		/// </summary>
		/// <param name="value">The boolean value.</param>
		public static implicit operator JsonSchema(bool value)
		{
			return value ? True : False;
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(JsonSchema other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			if (BoolValue.HasValue) return BoolValue == other.BoolValue;
			if (other.BoolValue.HasValue) return false;
			if (Keywords.Count != other.Keywords.Count) return false;
			if (OtherData.Count != other.OtherData.Count) return false;

			if (Keywords != null)
			{
				var byKeyword = Keywords.Join(other.Keywords,
						tk => tk.Keyword(),
						ok => ok.Keyword(),
						(tk, ok) => new {ThisKeyword = tk, OtherKeyword = ok})
					.ToList();
				if (byKeyword.Count != Keywords.Count) return false;
				if (!byKeyword.All(k => k.ThisKeyword.Equals(k.OtherKeyword))) return false;
			}

			if (OtherData != null)
			{
				var byKey = OtherData.Join(other.OtherData,
						td => td.Key,
						od => od.Key,
						(td, od) => new {ThisData = td.Value, OtherData = od.Value})
					.ToList();
				if (byKey.Count != OtherData.Count) return false;
				if (!byKey.All(k => k.ThisData.IsEquivalentTo(k.OtherData))) return false;
			}

			return true;
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as JsonSchema);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Keywords?.GetCollectionHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ (OtherData?.GetCollectionHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ BoolValue.GetHashCode();
				return hashCode;
			}
		}
	}

	internal class SchemaJsonConverter : JsonConverter<JsonSchema>
	{
		public override JsonSchema Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.True) return JsonSchema.True;
			if (reader.TokenType == JsonTokenType.False) return JsonSchema.False;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("JSON Schema must be true, false, or an object");

			if (!reader.Read())
				throw new JsonException("Expected token");

			var keywords = new List<IJsonSchemaKeyword>();
			var otherData = new Dictionary<string, JsonElement>();

			do
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.Comment:
						break;
					case JsonTokenType.PropertyName:
						var keyword = reader.GetString();
						reader.Read();
						var keywordType = SchemaKeywordRegistry.GetImplementationType(keyword);
						if (keywordType == null)
						{
							var element = JsonDocument.ParseValue(ref reader).RootElement;
							otherData[keyword] = element.Clone();
							break;
						}

						IJsonSchemaKeyword implementation;
						if (reader.TokenType == JsonTokenType.Null)
						{
							implementation = SchemaKeywordRegistry.GetNullValuedKeyword(keywordType);
							if (implementation == null)
								throw new InvalidOperationException($"No null instance registered for keyword `{keyword}`");
						}
						else
						{
							implementation = (IJsonSchemaKeyword)JsonSerializer.Deserialize(ref reader, keywordType, options);
							if (implementation == null)
								throw new InvalidOperationException($"Could not deserialize expected keyword `{keyword}`");
						}
						keywords.Add(implementation);
						break;
					case JsonTokenType.EndObject:
						return new JsonSchema(keywords, otherData);
					default:
						throw new JsonException("Expected keyword or end of schema object");
				}
			} while (reader.Read());

			throw new JsonException("Expected token");
		}

		public override void Write(Utf8JsonWriter writer, JsonSchema value, JsonSerializerOptions options)
		{
			if (value.BoolValue == true)
			{
				writer.WriteBooleanValue(true);
				return;
			}

			if (value.BoolValue == false)
			{
				writer.WriteBooleanValue(false);
				return;
			}

			writer.WriteStartObject();
			foreach (var keyword in value.Keywords)
			{
				JsonSerializer.Serialize(writer, keyword, keyword.GetType(), options);
			}

			if (value.OtherData != null)
			{
				foreach (var data in value.OtherData)
				{
					writer.WritePropertyName(data.Key);
					JsonSerializer.Serialize(writer, data.Value, options);
				}
			}

			writer.WriteEndObject();
		}
	}
}
