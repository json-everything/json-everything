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

		/// <summary>
		/// Gets the keyword priority.
		/// </summary>
		/// <param name="keyword">The keyword.</param>
		/// <returns>The priority.</returns>
		public static long Priority(this IJsonSchemaKeyword keyword)
		{
			if (keyword == null) throw new ArgumentNullException(nameof(keyword));

			var priorityAttribute = keyword.GetType().GetCustomAttribute<SchemaPriorityAttribute>();
			if (priorityAttribute == null) return 0;
			
			return priorityAttribute.ActualPriority;
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
	}
}