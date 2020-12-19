using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Logic
{
	public static class RuleRegistry
	{
		private static readonly Dictionary<string, Type> _rules =
			typeof(LogicComponent).Assembly
				.GetTypes()
				.Where(t => typeof(LogicComponent).IsAssignableFrom(t) && !t.IsAbstract)
				.ToDictionary(t => t.GetCustomAttribute<OperatorAttribute>().Name);

		public static Type GetRule(string identifier)
		{
			return _rules.TryGetValue(identifier, out var t) ? t : null;
		}
	}

	public class OperatorAttribute : Attribute
	{
		public string Name { get; }
		
		public OperatorAttribute(string name)
		{
			Name = name;
		}
	}
}