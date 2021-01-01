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
		/// <returns>The <see cref="Type"/> of the rule.</returns>
		public static Type GetRule(string identifier)
		{
			return _rules.TryGetValue(identifier, out var t) ? t : null;
		}
	}

	/// <summary>
	/// Decorates <see cref="Rule"/> implementations to identify a rule.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class OperatorAttribute : Attribute
	{
		/// <summary>
		/// The identifier.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Creates a new <see cref="OperatorAttribute"/> instance.
		/// </summary>
		/// <param name="name">The identifier.</param>
		public OperatorAttribute(string name)
		{
			Name = name;
		}
	}
}