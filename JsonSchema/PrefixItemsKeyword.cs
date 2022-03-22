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
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft202012)]
 	[Vocabulary(Vocabularies.Applicator202012Id)]
	[JsonConverter(typeof(PrefixItemsKeywordJsonConverter))]
	public class PrefixItemsKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaCollector, IEquatable<PrefixItemsKeyword>
	{
		internal const string Name = "prefixItems";

		/// <summary>
		/// The collection of schemas for the "schema array" form.
		/// </summary>
		public IReadOnlyList<JsonSchema> ArraySchemas { get; }

		IReadOnlyList<JsonSchema> ISchemaCollector.Schemas => ArraySchemas;

		static PrefixItemsKeyword()
		{
			ValidationResults.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		/// <summary>
		/// Creates a new <see cref="PrefixItemsKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of schemas for the "schema array" form.</param>
		/// <remarks>
		/// Using the `params` constructor to build an array-form `items` keyword with a single schema
		/// will confuse the compiler.  To achieve this, you'll need to explicitly specify the array.
		/// </remarks>
		public PrefixItemsKeyword(params JsonSchema[] values)
		{
			ArraySchemas = values?.ToList() ?? throw new ArgumentNullException(nameof(values));
		}

		/// <summary>
		/// Creates a new <see cref="PrefixItemsKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of schemas for the "schema array" form.</param>
		public PrefixItemsKeyword(IEnumerable<JsonSchema> values)
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
			var maxEvaluations = Math.Min(ArraySchemas.Count, context.LocalInstance.GetArrayLength());
			for (int i = 0; i < maxEvaluations; i++)
			{
				var schema = ArraySchemas[i];
				var item = context.LocalInstance[i];
				context.Push(context.InstanceLocation.Combine(PointerSegment.Create($"{i}")),
					item,
					context.SchemaLocation.Combine(PointerSegment.Create($"{i}")));
				schema.ValidateSubschema(context);
				overallResult &= context.LocalResult.IsValid;
				context.Pop();
				if (!overallResult && context.ApplyOptimizations) break;
			}

			if (overwriteAnnotation)
			{
				if (maxEvaluations == context.LocalInstance.GetArrayLength())
					context.LocalResult.SetAnnotation(Name, true);
				else
					context.LocalResult.SetAnnotation(Name, maxEvaluations);
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
			foreach (var schema in ArraySchemas)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(PrefixItemsKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (other.ArraySchemas == null) return false;

			return ArraySchemas.ContentsEqual(other.ArraySchemas);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as PrefixItemsKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return ArraySchemas.GetUnorderedCollectionHashCode();
		}
	}

	internal class PrefixItemsKeywordJsonConverter : JsonConverter<PrefixItemsKeyword>
	{
		public override PrefixItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartArray)
				throw new JsonException("Expected array");
			
			var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options);
			return new PrefixItemsKeyword(schemas);
		}
		public override void Write(Utf8JsonWriter writer, PrefixItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(PrefixItemsKeyword.Name);
			writer.WriteStartArray();
			foreach (var schema in value.ArraySchemas)
			{
				JsonSerializer.Serialize(writer, schema, options);
			}
			writer.WriteEndArray();
		}
	}
}