using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Json.Schema.Generation.XmlComments.Reflection;

/// <summary>
///     Collection of type information objects.
/// </summary>
public class TypeCollection
{
	/// <summary>
	///     Reflection settings that should be used when looking for referenced types.
	/// </summary>
	private ReflectionSettings _settings = ReflectionSettings.Default;

	/// <summary>
	///     Types that had their data and functions examined.
	/// </summary>
	private HashSet<Type> _visitedPropTypes = [];

	/// <summary>
	///     Types that need to have their properties, methods and fields examined.
	/// </summary>
	private Queue<Type> _pendingPropTypes = new();

	/// <summary>
	///     All referenced types.
	/// </summary>
	public Dictionary<Type, TypeInformation> ReferencedTypes { get; set; } = [];

	/// <summary>
	///     Get all types referenced by the specified type.
	///     Reflection information for the specified type is also returned.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="settings"></param>
	/// <returns></returns>
	public static TypeCollection ForReferencedTypes(Type type, ReflectionSettings? settings = null)
	{
		var typeCollection = new TypeCollection();
		typeCollection.GetReferencedTypes(type, settings);
		return typeCollection;
	}

	/// <summary>
	///     Get all types referenced by the specified type.
	///     Reflection information for the specified type is also returned.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="settings"></param>
	private void GetReferencedTypes(Type type, ReflectionSettings? settings = null)
	{
		Init(settings);
		_pendingPropTypes.Enqueue(type);
		ProcessTypeQueue();
	}

	private void Init(ReflectionSettings? settings)
	{
		_settings = settings ?? ReflectionSettings.Default;
		ReferencedTypes = [];
		_visitedPropTypes = [];
		_pendingPropTypes = new Queue<Type>();
	}

	private void ProcessTypeQueue()
	{
		while (_pendingPropTypes.Count > 0)
		{
			var theType = _pendingPropTypes.Dequeue();
			UnwrapType(null, theType);
			GetReferencedBy(theType);
		}
	}

	private void GetReferencedBy(Type type)
	{
		if (_visitedPropTypes.Contains(type)) return;
		_visitedPropTypes.Add(type);
		var thisTypeInfo = ReferencedTypes[type];
		foreach (var info in type.GetProperties(_settings.PropertyFlags))
		{
			if (_settings.PropertyFilter != null && !_settings.PropertyFilter(info)) continue;
			thisTypeInfo.Properties.Add(info);
			UnwrapType(type, info.PropertyType);
			if (info.GetMethod?.GetParameters().Length > 0)
				UnwrapType(type, info.GetMethod.GetParameters()[0].ParameterType);
			else if (info.SetMethod?.GetParameters().Length > 1) UnwrapType(type, info.SetMethod.GetParameters()[1].ParameterType);
		}

		foreach (var info in type.GetMethods(_settings.MethodFlags))
		{
			if (info.IsSpecialName) continue;
			if (_settings.MethodFilter != null && !_settings.MethodFilter(info)) continue;
			thisTypeInfo.Methods.Add(info);
			UnwrapType(type, info.ReturnType);
			if (!(info.GetParameters().Length > 0)) continue;
			foreach (var parameter in info.GetParameters()) UnwrapType(type, parameter.ParameterType);
		}

		foreach (var info in type.GetConstructors(_settings.MethodFlags))
		{
			if (_settings.MethodFilter != null && !_settings.MethodFilter(info)) continue;
			thisTypeInfo.Methods.Add(info);
			if (!(info.GetParameters().Length > 0)) continue;
			foreach (var parameter in info.GetParameters()) UnwrapType(type, parameter.ParameterType);
		}

		foreach (var info in type.GetFields(_settings.FieldFlags))
		{
			if (IsCompilerGenerated(info)) continue;
			if (_settings.FieldFilter != null && !_settings.FieldFilter(info)) continue;
			thisTypeInfo.Fields.Add(info);
			UnwrapType(type, info.FieldType);
		}

		foreach (var info in type.GetNestedTypes(_settings.NestedTypeFlags)) UnwrapType(type, info);
	}

	/// <summary>
	///     Recursively "unwrap" the generic type or array. If type is not generic and not an array
	///     then do nothing.
	/// </summary>
	/// <param name="parentType"></param>
	/// <param name="type"></param>
	private void UnwrapType(Type? parentType, Type type)
	{
		if (ReferencedTypes.ContainsKey(type))
		{
			if (parentType == null) return;
			return;
		}

		if (type.IsConstructedGenericType) // List<int>
		{
			UnwrapType(parentType, type.GetGenericTypeDefinition());
			if (!(type.GenericTypeArguments.Length > 0)) return;
			foreach (var argType in type.GenericTypeArguments) UnwrapType(parentType, argType);
		}
		else if (type.IsGenericParameter) // void Method<T>()   <-- T in generic class
		{
		}
		else if (type.IsGenericTypeDefinition) // List<>
		{
			AddTypeToCheckProps(type);
		}
		else if (type.IsGenericType) // List<int>
		{
			if (type.ContainsGenericParameters)
				foreach (var argType in type.GenericTypeArguments)
				{
					UnwrapType(parentType, argType);
				}
		}
		else if (type.IsArray || type.IsByRef) // SomeType[] or ref SomeType
		{
			UnwrapType(parentType, type.GetElementType()!);
		}
		else
		{
			AddTypeToCheckProps(type);
		}
	}

	private void AddTypeToCheckProps(Type type)
	{
		var newRef = new TypeInformation(type);
		ReferencedTypes.Add(type, newRef);
		_pendingPropTypes.Enqueue(type);
	}

	private static bool IsCompilerGenerated(FieldInfo fieldInfo)
	{
		return fieldInfo.FieldType.Name.Contains('<') ||
			   fieldInfo.CustomAttributes.Any(attr => attr.AttributeType == typeof(CompilerGeneratedAttribute));
	}

	/// <summary>
	///     Reflection information for the class, its methods, properties and fields.
	/// </summary>
	public class TypeInformation
	{
		public TypeInformation(Type type)
		{
			Type = type;
		}

		/// <summary>
		///     The type that this class describes
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		///     The list of property inforation of the class.
		/// </summary>
		public List<PropertyInfo> Properties { get; set; } = [];

		/// <summary>
		///     The list of method inforation of the class.
		/// </summary>
		public List<MethodBase> Methods { get; set; } = [];

		/// <summary>
		///     The list of field inforation of the class.
		/// </summary>
		public List<FieldInfo> Fields { get; set; } = [];
	}
}
