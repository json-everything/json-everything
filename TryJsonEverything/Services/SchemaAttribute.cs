using System;
using System.Linq;
using System.Reflection;
using Json.Schema;

namespace TryJsonEverything.Services
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
	public class SchemaAttribute : Attribute
	{
		public JsonSchema Schema { get; }

		public SchemaAttribute(Type hostType, string memberName)
		{
			var member = hostType.GetProperties()
				             .FirstOrDefault(x => IsStatic(x) && x.Name == memberName) ??
			             (MemberInfo?) hostType.GetFields()
				             .FirstOrDefault(x => x.IsStatic && x.Name == memberName) ??
			             throw new InvalidOperationException($"Cannot find static property or field named '{memberName}' on type '{hostType}'");

			var schema = (JsonSchema?) (member switch
			{
				PropertyInfo property => property.GetValue(null),
				FieldInfo field => field.GetValue(null),
				_ => null
			});

			Schema = schema ?? throw new InvalidOperationException($"Cannot find static property or field named '{memberName}' on type '{hostType}'");
		}

		private static bool IsStatic(PropertyInfo prop)
		{
			return prop.GetMethod?.IsStatic == true ||
			       prop.SetMethod?.IsStatic == true;
		}
	}
}