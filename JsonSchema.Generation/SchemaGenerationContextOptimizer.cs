using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using Json.More;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Provides meta-data about the generation process.
/// </summary>
public static class SchemaGenerationContextOptimizer
{
	/// <summary>
	/// Provides custom naming functionality.
	/// </summary>
	public static ITypeNameGenerator? TypeNameGenerator { get; set; }

	internal static void Optimize()
	{
		var allContexts = SchemaGenerationContextCache.Cache.Values;
		var root = allContexts.First();
		if (root is MemberGenerationContext)
			throw new InvalidOperationException("root should always be a type context");

		var contextsToMove = allContexts.Where(x => x.ReferenceCount > 1 && x.Intents.Count > 1);

		var currentNames = new List<string>();
		var defs = new Dictionary<string, SchemaGenerationContextBase>();
		foreach (var def in contextsToMove)
		{
			var name = GetDefName(def, currentNames);
			var refIntent = new RefIntent(new Uri(ReferenceEquals(def, root) ? "#" : $"#/$defs/{name}", UriKind.Relative));
			var refContext = new TypeGenerationContext(def.Type);
			refContext.Intents.Add(refIntent);
			foreach (var context in allContexts)
			{
				foreach (var intent in context.Intents.OfType<IContextContainer>())
				{
					intent.Replace(def.Hash, refContext);
				}

				if (ReferenceEquals(def, context)) continue;

				if (context is MemberGenerationContext memberContext)
				{
					if (def.Hash == memberContext.Hash)
					{
						context.Intents.Clear();
						context.Intents.Add(refIntent);
					}
				}
			}
			if (ReferenceEquals(def, root)) continue;
			defs[name] = def;
		}

		if (defs.Any())
		{
			var defsIntent = new DefsIntent(defs);
			root.Intents.Add(defsIntent);
		}
	}

	private static string GetDefName(SchemaGenerationContextBase context, List<string> currentNames)
	{
		var name = TypeNameGenerator?.GenerateName(context.Type) ?? GetName(context.Type).Camelize();
		var regex = new Regex($@"^{name}\d*$");
		var count = currentNames.Count(regex.IsMatch);
		if (count != 0)
			name += count;

		currentNames.Add(name);
		return name;
	}

	private static string GetName(Type type)
	{
		if (type.IsInteger()) return "integer";
		if (type.IsNumber()) return "number";
		if (type == typeof(string)) return "string";
		if (type == typeof(bool)) return "boolean";
		if (type.IsArray)
		{
			var itemType = type.GetElementType();
			return $"array of {GetName(itemType!)}";
		}
		if (type.IsGenericType &&
		    typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()) &&
		    type.GenericTypeArguments.Length == 1)
		{
			var itemType = type.GenericTypeArguments[0];
			return $"array of {GetName(itemType)}";
		}
		return type.GetFriendlyTypeName();
	}

	private static string GetFriendlyTypeName(this Type type, StringBuilder? sb = null)
	{
		sb ??= new StringBuilder();
		var name = type.Name;
		if (!type.IsGenericType)
		{
			if (type.IsNested && !type.IsGenericParameter)
				name = $"{name} in {GetName(type.DeclaringType!)}";
			return name;
		}

		sb.Append(name.Substring(0, name.IndexOf('`')));
		sb.Append(" of ");
		sb.Append(string.Join(" and ", type.GetGenericArguments()
			.Select(x => GetFriendlyTypeName(x, sb))));
		name = sb.ToString();

		if (type.IsNested)
			name = $"{name} in {GetName(type.DeclaringType!)}";

		return name;
	}
}