using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Logic
{
	/// <summary>
	/// Catalogs all of the known rules.
	/// </summary>
	public static class RuleRegistry
	{
		private static readonly Dictionary<string, Type> _rules =
			typeof(Rule).Assembly
				.GetTypes()
				.Where(t => typeof(Rule).IsAssignableFrom(t) && !t.IsAbstract)
				.SelectMany(t => t.GetCustomAttributes<OperatorAttribute>().Select(a => new {Name = a.Name, Type = t}))
				.ToDictionary(t => t.Name, t => t.Type);

		/// <summary>
		/// Gets a <see cref="Rule"/> implementation for a given identifier string.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <returns>The <see cref="System.Type"/> of the rule.</returns>
		public static Type? GetRule(string identifier)
		{
			return _rules.TryGetValue(identifier, out var t) ? t : null;
		}

		/// <summary>
		/// Registers a new rule type.
		/// </summary>
		/// <typeparam name="T">The type of the rule to add.</typeparam>
		/// <remarks>
		/// Rules must contain a parameterless constructor.
		///
		/// Decorate your rule type with one or more <see cref="OperatorAttribute"/>s to
		/// define its identifier.
		///
		/// Registering a rule with an identifier that already exists will overwrite the
		/// existing registration.
		/// </remarks>
		public static void AddRule<T>()
			where T : Rule
		{
			var type = typeof(T);
			var operators = type.GetCustomAttributes<OperatorAttribute>().Select(a => a.Name);
			foreach (var name in operators)
			{
				_rules[name] = type;
			}
		}
	}
}