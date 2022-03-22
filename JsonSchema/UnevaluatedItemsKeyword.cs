using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `unevaluatedItems`.
	/// </summary>
	[Applicator]
	[SchemaPriority(30)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[Vocabulary(Vocabularies.Applicator202012Id)]
	[JsonConverter(typeof(UnevaluatedItemsKeywordJsonConverter))]
	public class UnevaluatedItemsKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, IEquatable<UnevaluatedItemsKeyword>
	{
		internal const string Name = "unevaluatedItems";

		/// <summary>
		/// The schema by which to validation unevaluated items.
		/// </summary>
		public JsonSchema Schema { get; }

		static UnevaluatedItemsKeyword()
		{
			ValidationResults.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		/// <summary>
		/// Creates a new <see cref="UnevaluatedItemsKeyword"/>.
		/// </summary>
		/// <param name="value">The schema by which to validation unevaluated items.</param>
		public UnevaluatedItemsKeyword(JsonSchema value)
		{
			Schema = value ?? throw new ArgumentNullException(nameof(value));
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

			context.Options.LogIndentLevel++;
			var overallResult = true;
			int startIndex = 0;
			object? annotation;
			if (context.Options.ValidatingAs == Draft.Unspecified || context.Options.ValidatingAs.HasFlag(Draft.Draft202012))
			{
				annotation = context.LocalResult.TryGetAnnotation(PrefixItemsKeyword.Name);
				if (annotation != null)
				{
					context.Log(() => $"Annotation from {PrefixItemsKeyword.Name}: {annotation}.");
					if (annotation is bool) // is only ever true or a number
					{
						context.LocalResult.Pass();
						context.ExitKeyword(Name, true);
						return;
					}
					startIndex = (int)annotation;
				}
				else
					context.Log(() => $"No annotations from {PrefixItemsKeyword.Name}.");
			}
			annotation = context.LocalResult.TryGetAnnotation(ItemsKeyword.Name);
			if (annotation != null)
			{
				context.Log(() => $"Annotation from {ItemsKeyword.Name}: {annotation}.");
				if (annotation is bool) // is only ever true or a number
				{
					context.LocalResult.Pass();
					context.ExitKeyword(Name, true);
					return;
				}
				startIndex = (int) annotation;
			}
			else
				context.Log(() => $"No annotations from {ItemsKeyword.Name}.");
			annotation = context.LocalResult.TryGetAnnotation(AdditionalItemsKeyword.Name);
			if (annotation is bool) // is only ever true
			{
				context.Log(() => $"Annotation from {AdditionalItemsKeyword.Name}: {annotation}.");
				context.LocalResult.Pass();
				context.ExitKeyword(Name, true);
				return;
			}
			context.Log(() => $"No annotations from {AdditionalItemsKeyword.Name}.");
			annotation = context.LocalResult.TryGetAnnotation(Name);
			if (annotation is bool) // is only ever true
			{
				context.Log(() => $"Annotation from {Name}: {annotation}.");
				context.LocalResult.Pass();
				context.ExitKeyword(Name, true);
				return;
			}
			context.Log(() => $"No annotations from {Name}.");
			var indicesToValidate = Enumerable.Range(startIndex, context.LocalInstance.GetArrayLength() - startIndex);
			if (context.Options.ValidatingAs.HasFlag(Draft.Draft202012) || context.Options.ValidatingAs == Draft.Unspecified)
			{
				var validatedByContains = context.LocalResult.GetAllAnnotations<List<int>>(ContainsKeyword.Name).SelectMany(x => x).ToList();
				if (validatedByContains.Any())
				{
					context.Log(() => $"Annotation from {ContainsKeyword.Name}: {annotation}.");
					indicesToValidate = indicesToValidate.Except(validatedByContains);
				}
				else
					context.Log(() => $"No annotations from {ContainsKeyword.Name}.");
			}
			foreach (var i in indicesToValidate)
			{
				context.Log(() => $"Validating item at index {i}.");
				var item = context.LocalInstance[i];
				context.Push(context.InstanceLocation.Combine(PointerSegment.Create($"{i}")), item);
				Schema.ValidateSubschema(context);
				overallResult &= context.LocalResult.IsValid;
				context.Log(() => $"Item at index {i} {context.LocalResult.IsValid.GetValidityString()}.");
				context.Pop();
				if (!overallResult && context.ApplyOptimizations) break;
			}
			context.Options.LogIndentLevel--;

			context.LocalResult.SetAnnotation(Name, true);
			if (overallResult)
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail();
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		private static void ConsolidateAnnotations(ValidationResults localResults)
		{
			if (localResults.NestedResults.Select(c => c.TryGetAnnotation(Name)).OfType<bool>().Any())
				localResults.SetAnnotation(Name, true);
		}

		IRefResolvable? IRefResolvable.ResolvePointerSegment(string? value)
		{
			throw new NotImplementedException();
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			Schema.RegisterSubschemas(registry, currentUri);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(UnevaluatedItemsKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Schema, other.Schema);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as UnevaluatedItemsKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Schema.GetHashCode();
		}
	}

	internal class UnevaluatedItemsKeywordJsonConverter : JsonConverter<UnevaluatedItemsKeyword>
	{
		public override UnevaluatedItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new UnevaluatedItemsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, UnevaluatedItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(UnevaluatedItemsKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}