using System.Reflection;
using System.Text.RegularExpressions;

namespace JsonEverythingNet.Services.MarkdownGen;

/// <summary>
///     Class that constructs IDs for XML documentation comments.
///     IDs uniquely identify comments in the XML documentation file.
/// </summary>
public static class XmlDocId
{
	/// <summary>
	///     Type member XML ID prefix.
	/// </summary>
	private const char _memberPrefix = 'M';

	/// <summary>
	///     Field name XML ID prefix.
	/// </summary>
	private const char _fieldPrefix = 'F';

	/// <summary>
	///     Property name XML ID prefix.
	/// </summary>
	private const char _propertyPrefix = 'P';

	/// <summary>
	///     Event XML ID prefix.
	/// </summary>
	private const char _eventPrefix = 'E';

	/// <summary>
	///     Type name XML ID prefix.
	/// </summary>
	private const char _typePrefix = 'T';

	/// <summary>
	///     Part of the constructor XML tag in XML document.
	/// </summary>
	private const string _constructorNameId = "#ctor";

	/// <summary>
	///     Get XML Id of the type definition.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public static string TypeId(this Type type)
	{
		return _typePrefix + ":" + GetTypeXmlId(type);
	}

	/// <summary>
	///     Get XML Id of a class method
	/// </summary>
	/// <param name="methodInfo"></param>
	/// <returns></returns>
	public static string MethodId(this MethodBase? methodInfo)
	{
		if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
		return (IsIndexerProperty(methodInfo) ? _propertyPrefix : _memberPrefix) + ":" +
		       GetTypeXmlId(methodInfo.DeclaringType!) + "." +
		       GetMethodXmlId(methodInfo);
	}

	/// <summary>
	///     Get XML Id of any member of the type.
	/// </summary>
	/// <param name="memberInfo"></param>
	/// <returns></returns>
	public static string MemberId(this MemberInfo memberInfo)
	{
		if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
		switch (memberInfo.MemberType)
		{
			case MemberTypes.Constructor:
			case MemberTypes.Method:
				return MethodId(memberInfo as MethodBase);
			case MemberTypes.Property:
				return PropertyId(memberInfo);
			case MemberTypes.Field:
				return FieldId(memberInfo);
			case MemberTypes.NestedType:
				return TypeId((Type)memberInfo);
			case MemberTypes.Event:
				return EventId(memberInfo);
			// case MemberTypes.TypeInfo:
		}

		throw new NotSupportedException($"{memberInfo.MemberType}");
	}

	/// <summary>
	///     Get XML Id of property
	/// </summary>
	/// <param name="propertyInfo"></param>
	/// <returns></returns>
	public static string PropertyId(this MemberInfo propertyInfo)
	{
		if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
		if ((propertyInfo.MemberType & MemberTypes.Property) == 0) throw new ArgumentException(nameof(propertyInfo));

		if (propertyInfo.Name == "Item")
		{
			var getParameters = (propertyInfo as PropertyInfo)?.GetMethod?.GetParameters();
			if (getParameters?.Length > 0)
				return _propertyPrefix + ":" + GetTypeXmlId(propertyInfo.DeclaringType!) + "." +
				       propertyInfo.Name +
				       GetParametersXmlId(getParameters, GetGenericClassParams(propertyInfo));

			var setParameters = (propertyInfo as PropertyInfo)?.SetMethod?.GetParameters();
			if (setParameters?.Length > 1)
				return _propertyPrefix + ":" + GetTypeXmlId(propertyInfo.DeclaringType!) + "." +
				       propertyInfo.Name +
				       GetParametersXmlId(setParameters.Take(setParameters.Length - 1), GetGenericClassParams(propertyInfo));
		}

		return _propertyPrefix + ":" + GetTypeXmlId(propertyInfo.DeclaringType!) + "." + propertyInfo.Name;
	}

	/// <summary>
	///     Get XML Id of field
	/// </summary>
	/// <param name="fieldInfo"></param>
	/// <returns></returns>
	public static string FieldId(this MemberInfo fieldInfo)
	{
		if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));
		if ((fieldInfo.MemberType & MemberTypes.Field) == 0) throw new ArgumentException(nameof(fieldInfo));
		return _fieldPrefix + ":" + GetTypeXmlId(fieldInfo.DeclaringType!) + "." + fieldInfo.Name;
	}

	/// <summary>
	///     Get XML Id of event field
	/// </summary>
	/// <param name="eventInfo"></param>
	/// <returns></returns>
	public static string EventId(this MemberInfo eventInfo)
	{
		if (eventInfo == null) throw new ArgumentNullException(nameof(eventInfo));
		if ((eventInfo.MemberType & MemberTypes.Event) == 0) throw new ArgumentException(nameof(eventInfo));
		return _eventPrefix + ":" + GetTypeXmlId(eventInfo.DeclaringType!) + "." + eventInfo.Name;
	}

	/// <summary>
	///     Get XML Id of specified value of the enum type.
	/// </summary>
	/// <param name="enumType">Enum type</param>
	/// <param name="enumName">The name of the value without type and namespace</param>
	/// <returns></returns>
	public static string EnumValueId(this Type enumType, string enumName)
	{
		if (enumType == null) throw new ArgumentNullException(nameof(enumType));
		if (enumName == null) throw new ArgumentNullException(nameof(enumName));
		if (!enumType.IsEnum) throw new ArgumentException(nameof(enumType));
		return _fieldPrefix + ":" + GetTypeXmlId(enumType) + "." + enumName;
	}

	#region Non-public functions

	/// <summary>
	///     Gets the type's full name prepared for xml documentation format.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="isOut">Whether the declaring member for this type is an out directional parameter.</param>
	/// <param name="isMethodParameter">If the type is being used has a method parameter.</param>
	/// <param name="genericClassParams">The names of the generic class parameters from the parent type.</param>
	/// <returns>The full name.</returns>
	private static string GetTypeXmlId(Type type, bool isOut = false, bool isMethodParameter = false, string[]? genericClassParams = null)
	{
		// Generic parameters of the class  are referred to as  `N where N is position of generic type.
		// Generic parameters of the method are referred to as ``N where N is position of generic type.
		if (type.IsGenericParameter) return $"{GenericParamPrefix(type, genericClassParams)}{type.GenericParameterPosition}";

		var args = type.GetGenericArguments();
		string fullTypeName;
		var typeNamespace = type.Namespace == null ? "" : $"{type.Namespace}.";
		var outString = isOut ? "@" : "";

		if (type.MemberType == MemberTypes.TypeInfo &&
		    !type.IsGenericTypeDefinition &&
		    (type.IsGenericType || args.Length > 0) && (!type.IsClass || isMethodParameter))
		{
			var paramString = string.Join(",",
				args.Select(o => GetTypeXmlId(o, false, isMethodParameter, genericClassParams)));
			var typeName = Regex.Replace(type.Name, "`[0-9]+", "{" + paramString + "}");
			fullTypeName = $"{typeNamespace}{typeName}{outString}";
		}
		else if (type.IsNested)
		{
			fullTypeName = $"{typeNamespace}{type.DeclaringType!.Name}.{type.Name}{outString}";
		}
		else
		{
			fullTypeName = $"{typeNamespace}{type.Name}{outString}";
		}

		fullTypeName = fullTypeName.Replace("&", "");

		// Multi-dimensional arrays must have 0: for each dimension. Eg. [,,] has to become [0:,0:,0:]
		while (fullTypeName.Contains("[,"))
		{
			var index = fullTypeName.IndexOf("[,", StringComparison.Ordinal);
			var lastIndex = fullTypeName.IndexOf(']', index);
			fullTypeName = fullTypeName[..(index + 1)] +
			               string.Join(",", Enumerable.Repeat("0:", lastIndex - index)) +
			               fullTypeName[lastIndex..];
		}

		return fullTypeName;
	}

	/// <summary>
	///     Get method element Id in XML document
	/// </summary>
	/// <param name="methodInfo"></param>
	/// <returns></returns>
	private static string GetMethodXmlId(MethodBase methodInfo)
	{
		return $"{ShortMethodName(methodInfo)}" +
		       GetParametersXmlId(methodInfo.GetParameters(), GetGenericClassParams(methodInfo)) +
		       ExplicitImplicitPostfix(methodInfo);
	}

	private static string GetParametersXmlId(IEnumerable<ParameterInfo> parameters, string[] genericClassParams)
	{
		// Calculate the parameter string as this is in the member name in the XML
		var parameterStrings = parameters
			.Select(parameterInfo => GetTypeXmlId(
				parameterInfo.ParameterType,
				parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef,
				true,
				genericClassParams))
			.ToList();
		return parameterStrings.Count > 0 ? $"({string.Join(",", parameterStrings)})" : "";
	}

	/// <summary>
	///     Return true if this method is actually an indexer property.
	/// </summary>
	/// <param name="methodInfo"></param>
	/// <returns></returns>
	private static bool IsIndexerProperty(MethodBase methodInfo)
	{
		return methodInfo.IsSpecialName && (methodInfo.Name == "get_Item" || methodInfo.Name == "set_Item");
	}

	/// <summary>
	///     Explicit/implicit operator may have return value appended to the name.
	/// </summary>
	/// <param name="methodInfo"></param>
	/// <returns></returns>
	private static string ExplicitImplicitPostfix(MethodBase methodInfo)
	{
		if (!methodInfo.IsSpecialName ||
		    methodInfo.Name != "op_Explicit" && methodInfo.Name != "op_Implicit") return "";
		return "~" + GetTypeXmlId(((MethodInfo)methodInfo).ReturnType);
	}

	/// <summary>
	///     Get method name. Some methods have special names or like generic methods some extra information.
	/// </summary>
	/// <param name="methodInfo"></param>
	/// <returns></returns>
	private static string ShortMethodName(MethodBase methodInfo)
	{
		if (methodInfo.IsConstructor) return _constructorNameId;
		return (IsIndexerProperty(methodInfo) ? "Item" : methodInfo.Name) +
		       (methodInfo.IsGenericMethod ? "``" + methodInfo.GetGenericArguments().Length : "");
	}

	private static string[] GetGenericClassParams(MemberInfo info)
	{
		return info.DeclaringType!.IsGenericType
			? info.DeclaringType.GetGenericArguments().Select(t => t.Name).ToArray()
			: Array.Empty<string>();
	}

	private static string GenericParamPrefix(Type type, string[]? genericClassParams)
	{
		return genericClassParams != null && genericClassParams.Contains(type.Name) ? "`" : "``";
	}

	#endregion
}