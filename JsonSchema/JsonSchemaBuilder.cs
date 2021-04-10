using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema
{
	/// <summary>
	/// A fluent-style builder for <see cref="JsonSchema"/>.
	/// </summary>
	public class JsonSchemaBuilder
	{
		private readonly Dictionary<string, IJsonSchemaKeyword> _keywords = new Dictionary<string, IJsonSchemaKeyword>();

		/// <summary>
		/// Adds a new keyword.
		/// </summary>
		/// <param name="keyword">The keyword to add.</param>
		public void Add(IJsonSchemaKeyword keyword)
		{
			_keywords[keyword.Keyword()] = keyword;
		}

		/// <summary>
		/// Gets a keyword if one has been added.
		/// </summary>
		/// <typeparam name="T">the keyword type.</typeparam>
		/// <returns>The keyword, if it exists; `null` otherwise.</returns>
		public T? Get<T>()
			where T : IJsonSchemaKeyword
		{
			return _keywords.Values.OfType<T>().SingleOrDefault();
		}

		/// <summary>
		/// Creates a new <see cref="JsonSchema"/>.
		/// </summary>
		/// <returns>A JSON Schema that simply refers back to the root schema.</returns>
		public static JsonSchema RefRoot()
		{
			return new JsonSchemaBuilder().Ref(new Uri("#", UriKind.RelativeOrAbsolute)).Build();
		}

		/// <summary>
		/// Creates a new <see cref="JsonSchema"/>.
		/// </summary>
		/// <returns>A JSON Schema that simply refers back to the recursive root schema.</returns>
		public static JsonSchema RecursiveRefRoot()
		{
			return new JsonSchemaBuilder().RecursiveRef(new Uri("#", UriKind.RelativeOrAbsolute)).Build();
		}

		/// <summary>
		/// Builds the schema.
		/// </summary>
		/// <returns>A <see cref="JsonSchema"/>.</returns>
		public JsonSchema Build()
		{
			var duplicates = _keywords.GroupBy(k => k.Value.GetType())
				.Where(g => g.Count() > 1)
				.Select(g => g.Key.GetCustomAttribute<SchemaKeywordAttribute>()?.Name)
				.ToList();
			if (duplicates.Any())
				throw new ArgumentException($"Found duplicate keywords: [{string.Join(", ", duplicates)}]");

			return new JsonSchema(_keywords.Values, null);
		}

		/// <summary>
		/// For convenience, implicitly calls <see cref="Build()"/>.
		/// </summary>
		/// <returns>A <see cref="JsonSchema"/>.</returns>
		public static implicit operator JsonSchema(JsonSchemaBuilder builder)
		{
			return builder.Build();
		}
	}
}