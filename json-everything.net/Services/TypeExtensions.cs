using System.Text;

namespace JsonEverythingNet.Services;

public static class TypeExtensions
{
	private static readonly Dictionary<Type, string> _keywordedTypes =
		new()
		{
			[typeof(void)] = "void",
			[typeof(string)] = "string",
			[typeof(int)] = "int",
			[typeof(short)] = "short",
			[typeof(long)] = "long",
			[typeof(ulong)] = "ulong",
			[typeof(uint)] = "uint",
			[typeof(ushort)] = "ushort",
			[typeof(double)] = "double",
			[typeof(float)] = "float",
			[typeof(byte)] = "byte",
			[typeof(char)] = "char",
			[typeof(bool)] = "bool",
		};

	public static string CSharpName(this Type type, StringBuilder? sb = null)
	{
		if (_keywordedTypes.TryGetValue(type, out var keyword)) return keyword;

		sb ??= new StringBuilder();
		var name = type.Name;
		if (!type.IsGenericType)
		{
			if (type.IsNested && !type.IsGenericParameter)
				name = $"{type.DeclaringType!.CSharpName()}.{name}";
			return name;
		}

		if (type.GetGenericTypeDefinition() == typeof(Nullable<>)) return $"{CSharpName(type.GetGenericArguments()[0])}?";

		sb.Append(name[..name.IndexOf('`')]);
		sb.Append('<');
		sb.Append(string.Join(", ", type.GetGenericArguments()
			.Select(x => CSharpName(x, sb))));
		sb.Append('>');
		name = sb.ToString();

		if (type.IsNested)
			name = $"{type.DeclaringType!.CSharpName()}.{name}";

		return name;
	}
}