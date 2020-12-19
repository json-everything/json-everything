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
				.SelectMany(t => t.GetCustomAttributes<OperatorAttribute>().Select(a => new {Name = a.Name, Type = t}))
				.ToDictionary(t => t.Name, t => t.Type);

		public static Type GetRule(string identifier)
		{
			return _rules.TryGetValue(identifier, out var t) ? t : null;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class OperatorAttribute : Attribute
	{
		public string Name { get; }
		
		public OperatorAttribute(string name)
		{
			Name = name;
		}
	}
}