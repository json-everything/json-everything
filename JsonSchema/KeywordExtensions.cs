using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema
{
	/// <summary>
	/// Some extensions for <see cref="IJsonSchemaKeyword"/>.
	/// </summary>
	public static class KeywordExtensions
	{
		private static readonly Dictionary<Type, string> _attributes =
			typeof(IJsonSchemaKeyword).Assembly
				.GetTypes()
				.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
				            !t.IsAbstract &&
				            !t.IsInterface)
				.ToDictionary(t => t, t => t.GetCustomAttribute<SchemaKeywordAttribute>().Name);

		/// <summary>
		/// Gets the keyword string.
		/// </summary>
		/// <param name="keyword">The keyword.</param>
		/// <returns>The keyword string.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="keyword"/> is null.</exception>
		/// <exception cref="InvalidOperationException">The keyword does not carry the <see cref="SchemaKeywordAttribute"/>.</exception>
		public static string Keyword(this IJsonSchemaKeyword keyword)
		{
			if (keyword == null) throw new ArgumentNullException(nameof(keyword));

			var keywordType = keyword.GetType();
			if (!_attributes.TryGetValue(keywordType, out var name))
			{
				name = keywordType.GetCustomAttribute<SchemaKeywordAttribute>()?.Name;
				if (name == null)
					throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaKeywordAttribute)}");

				_attributes[keywordType] = name;
			}

			return name;
		}

		private static readonly Dictionary<Type, long> _priorities =
            typeof(IJsonSchemaKeyword).Assembly
                .GetTypes()
                .Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
                            !t.IsAbstract &&
                            !t.IsInterface)
                .ToDictionary(t => t, t => t.GetCustomAttribute<SchemaPriorityAttribute>()?.ActualPriority ?? 0);

		/// <summary>
		/// Gets the keyword priority.
		/// </summary>
		/// <param name="keyword">The keyword.</param>
		/// <returns>The priority.</returns>
		public static long Priority(this IJsonSchemaKeyword keyword)
		{
			if (keyword == null) throw new ArgumentNullException(nameof(keyword));

            var keywordType = keyword.GetType();
            if (!_priorities.TryGetValue(keywordType, out var priority))
            {
                var priorityAttribute = keywordType.GetCustomAttribute<SchemaPriorityAttribute>();
                priority = priorityAttribute?.ActualPriority ?? 0;
				_priorities[keywordType] = priority;
            }

			return priority;
		}

		private static readonly Dictionary<Type, Draft> _draftDeclarations =
			typeof(IJsonSchemaKeyword).Assembly
				.GetTypes()
				.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
				            !t.IsAbstract &&
				            !t.IsInterface)
				.ToDictionary(t => t, t => t.GetCustomAttributes<SchemaDraftAttribute>()
					.Aggregate(Draft.Unspecified, (c, x) => c | x.Draft));

		/// <summary>
		/// Determines if a keyword is declared by a given draft of the JSON Schema specification.
		/// </summary>
		/// <param name="keyword">The keyword.</param>
		/// <param name="draft">The queried draft.</param>
		/// <returns>true if the keyword is supported by the draft; false otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="keyword"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the keyword has no <see cref="SchemaDraftAttribute"/> declarations.</exception>
		public static bool SupportsDraft(this IJsonSchemaKeyword keyword, Draft draft)
		{
			if (keyword == null) throw new ArgumentNullException(nameof(keyword));

			var keywordType = keyword.GetType();
			if (!_draftDeclarations.TryGetValue(keywordType, out var supportedDrafts))
			{
				supportedDrafts = keywordType.GetCustomAttributes<SchemaDraftAttribute>()
					.Aggregate(Draft.Unspecified, (c, x) => c | x.Draft);
				if (supportedDrafts == Draft.Unspecified)
					throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaDraftAttribute)}");

				_draftDeclarations[keywordType] = supportedDrafts;
			}

			return supportedDrafts.HasFlag(draft);
		}

		/// <summary>
		/// Gets whether the keyword is an applicator (carries the <see cref="ApplicatorAttribute"/> attribute).
		/// </summary>
		/// <param name="keyword">The keyword.</param>
		/// <returns><code>true</code> if the keyword is an applicator; <code>false</code> otherwise.</returns>
		public static bool IsApplicator(this IJsonSchemaKeyword keyword)
		{
			return keyword.GetType().GetCustomAttribute<ApplicatorAttribute>() != null;
		}

		/// <summary>
		/// Gets all immediate subschemas for a keyword.
		/// </summary>
		/// <param name="keyword">The keyword.</param>
		/// <returns>An `IEnumerable&lt;JsonSchema&gt;`.</returns>
		public static IEnumerable<JsonSchema> GetSubschemas(this IJsonSchemaKeyword keyword)
		{
			return keyword switch
			{
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				ISchemaContainer container => container.Schema == null ? Enumerable.Empty<JsonSchema>() : new[] {container.Schema},
				// ReSharper disable ConstantNullCoalescingCondition
				ISchemaCollector collector => collector.Schemas ?? Enumerable.Empty<JsonSchema>(),
				IKeyedSchemaCollector collector => collector.Schemas.Values ?? Enumerable.Empty<JsonSchema>(),
				// ReSharper restore ConstantNullCoalescingCondition
				_ => Enumerable.Empty<JsonSchema>()
			};
		}
	}
}