using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `items`.
	/// </summary>
	[Applicator]
	[SchemaPriority(5)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
 	[Vocabulary(Vocabularies.Applicator202012Id)]
	[JsonConverter(typeof(ItemsKeywordJsonConverter))]
	public class ItemsKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, ISchemaCollector, IEquatable<ItemsKeyword>
	{
		internal const string Name = "items";

		/// <summary>
		/// The schema for the "single schema" form.
		/// </summary>
		public JsonSchema? SingleSchema { get; }

		JsonSchema ISchemaContainer.Schema => SingleSchema!;

		/// <summary>
		/// The collection of schemas for the "schema array" form.
		/// </summary>
		public IReadOnlyList<JsonSchema>? ArraySchemas { get; }

		IReadOnlyList<JsonSchema> ISchemaCollector.Schemas => ArraySchemas!;

		static ItemsKeyword()
		{
			ValidationResults.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		/// <summary>
		/// Creates a new <see cref="ItemsKeyword"/>.
		/// </summary>
		/// <param name="value">The schema for the "single schema" form.</param>
		public ItemsKeyword(JsonSchema value)
		{
			SingleSchema = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Creates a new <see cref="ItemsKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of schemas for the "schema array" form.</param>
		/// <remarks>
		/// Using the `params` constructor to build an array-form `items` keyword with a single schema
		/// will confuse the compiler.  To achieve this, you'll need to explicitly specify the array.
		/// </remarks>
		public ItemsKeyword(params JsonSchema[] values)
		{
			ArraySchemas = values.ToList();
		}

		/// <summary>
		/// Creates a new <see cref="ItemsKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of schemas for the "schema array" form.</param>
		public ItemsKeyword(IEnumerable<JsonSchema> values)
		{
			ArraySchemas = values.ToList();
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			if (context.LocalInstance.ValueKind != JsonValueKind.Array)
			{
				context.LocalResult.Pass();
				context.WrongValueKind(context.LocalInstance.ValueKind);
				return;
			}

			bool overwriteAnnotation = !(context.LocalResult.TryGetAnnotation(Name) is bool);
			var overallResult = true;
			if (SingleSchema != null)
			{
				context.Options.LogIndentLevel++;
				int startIndex;
				var annotation = context.LocalResult.TryGetAnnotation(PrefixItemsKeyword.Name);
				if (annotation == null)
					startIndex = 0;
				else
				{
					context.Log(() => $"Annotation from {PrefixItemsKeyword.Name}: {annotation}");
					if (annotation is bool)
					{
						context.LocalResult.Pass();
						context.ExitKeyword(Name, true);
						return;
					}

					startIndex = (int) annotation;
				}

				for (int i = startIndex; i < context.LocalInstance.GetArrayLength(); i++)
				{
					context.Log(() => $"Validating item at index {i}.");
					var item = context.LocalInstance[i];
					context.Push(context.InstanceLocation.Combine(PointerSegment.Create($"{i}")), item);
					SingleSchema.ValidateSubschema(context);
					overallResult &= context.LocalResult.IsValid;
					context.Log(() => $"Item at index {i} {context.LocalResult.IsValid.GetValidityString()}.");
					context.Pop();
					if (!overallResult && context.ApplyOptimizations) break;
				}
				context.Options.LogIndentLevel--;

				if (overwriteAnnotation)
				{
					// TODO: add message
					context.LocalResult.SetAnnotation(Name, true);
				}
			}
			else // array
			{
				if (context.Options.ValidatingAs == Draft.Draft202012)
				{
					context.LocalResult.Fail($"Array form of {Name} is invalid for draft 2020-12 and later");
					context.Log(() => $"Array form of {Name} is invalid for draft 2020-12 and later");
					context.ExitKeyword(Name, false);
					return;
				}
				context.Options.LogIndentLevel++;
				var maxEvaluations = Math.Min(ArraySchemas!.Count, context.LocalInstance.GetArrayLength());
				for (int i = 0; i < maxEvaluations; i++)
				{
					context.Log(() => $"Validating item at index {i}.");
					var schema = ArraySchemas[i];
					var item = context.LocalInstance[i];
					context.Push(context.InstanceLocation.Combine(PointerSegment.Create($"{i}")),
						item,
						context.SchemaLocation.Combine(PointerSegment.Create($"{i}")));
					schema.ValidateSubschema(context);
					overallResult &= context.LocalResult.IsValid;
					context.Log(() => $"Item at index {i} {context.LocalResult.IsValid.GetValidityString()}.");
					context.Pop();
					if (!overallResult && context.ApplyOptimizations) break;
				}
				context.Options.LogIndentLevel--;

				if (overwriteAnnotation)
				{
					// TODO: add message
					if (maxEvaluations == context.LocalInstance.GetArrayLength())
						context.LocalResult.SetAnnotation(Name, true);
					else
						context.LocalResult.SetAnnotation(Name, maxEvaluations);
				}
			}

			if (overallResult)
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail();
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		private static void ConsolidateAnnotations(ValidationResults localResults)
		{
			object value;
			var allAnnotations = localResults.NestedResults.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.ToList();
			if (allAnnotations.OfType<bool>().Any())
				value = true;
			else
				value = allAnnotations.OfType<int>().DefaultIfEmpty(-1).Max();
			if (!Equals(value, -1))
				localResults.SetAnnotation(Name, value);
		}

		IRefResolvable? IRefResolvable.ResolvePointerSegment(string? value)
		{
			throw new NotImplementedException();
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			if (SingleSchema != null)
				SingleSchema.RegisterSubschemas(registry, currentUri);
			else
			{
				foreach (var schema in ArraySchemas!)
				{
					schema.RegisterSubschemas(registry, currentUri);
				}
			}
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(ItemsKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (SingleSchema != null)
			{
				if (other.SingleSchema == null) return false;
				return Equals(SingleSchema, other.SingleSchema);
			}

			if (ArraySchemas != null)
			{
				if (other.ArraySchemas == null) return false;
				return ArraySchemas.ContentsEqual(other.ArraySchemas);
			}

			throw new InvalidOperationException("Either SingleSchema or ArraySchemas should be populated.");
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as ItemsKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = SingleSchema?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ (ArraySchemas?.GetUnorderedCollectionHashCode() ?? 0);
				return hashCode;
			}
		}
	}

	internal class ItemsKeywordJsonConverter : JsonConverter<ItemsKeyword>
	{
		public override ItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options);
				return new ItemsKeyword(schemas);
			}
			
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
			return new ItemsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, ItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ItemsKeyword.Name);
			if (value.SingleSchema != null)
				JsonSerializer.Serialize(writer, value.SingleSchema, options);
			else
			{
				writer.WriteStartArray();
				foreach (var schema in value.ArraySchemas!)
				{
					JsonSerializer.Serialize(writer, schema, options);
				}
				writer.WriteEndArray();
			}
		}
	}
}