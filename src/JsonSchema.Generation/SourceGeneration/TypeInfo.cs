using System.Collections.Generic;
using Json.Schema.Generation.Serialization;
using Microsoft.CodeAnalysis;

namespace Json.Schema.Generation.SourceGeneration;

internal sealed class TypeInfo
{
	public required ITypeSymbol TypeSymbol { get; init; }
	public required string FullyQualifiedName { get; init; }
	public required string SchemaPropertyName { get; init; }
	public string? ResolvedPropertyName { get; set; }
	public required NamingConvention PropertyNaming { get; init; }
	public required PropertyOrder PropertyOrder { get; init; }
	public required bool StrictConditionals { get; init; }
	public required TypeKind Kind { get; init; }
	public required bool IsNullable { get; init; }
	public List<PropertyInfo> Properties { get; init; } = new();
	public List<string> EnumValues { get; init; } = new();
	public string? XmlDocSummary { get; init; }
	public List<AttributeInfo> TypeAttributes { get; init; } = new();
	public List<ConditionalInfo> Conditionals { get; init; } = new();
	public List<AttributeInfo>? ItemAttributes { get; init; }
	public List<AttributeInfo>? PropertyAttributes { get; init; }
}

internal sealed class PropertyInfo
{
	public required string Name { get; init; }
	public required string SchemaName { get; init; }
	public required ITypeSymbol Type { get; init; }
	public required bool IsRequired { get; init; }
	public required bool IsNullable { get; init; }
	public required bool IsReadOnly { get; init; }
	public required bool IsWriteOnly { get; init; }
	public List<AttributeInfo> Attributes { get; init; } = new();
	public string? XmlDocSummary { get; init; }
	public List<object> ConditionGroups { get; init; } = new();
}

internal sealed class AttributeInfo
{
	public required string AttributeName { get; init; }
	public Dictionary<string, object?> Parameters { get; init; } = new();
	public bool IsCustomEmitter { get; init; }
	public string? AttributeFullName { get; init; }
	public List<ApplyParameterInfo>? ApplyMethodParameters { get; init; }
}

internal sealed class ApplyParameterInfo
{
	public required string Name { get; init; }
	public required string TypeName { get; init; }
}

internal sealed class SchemaHandlerInfo
{
	public required string HandlerTypeName { get; init; }
	public required string TargetTypeName { get; init; }
	public required bool IsOpenGenericTarget { get; init; }
	public required bool ReturnsBuilder { get; init; }
}

internal enum TypeKind
{
	Unknown,
	Any,
	Dictionary,
	Boolean,
	Integer,
	Number,
	String,
	DateTime,
	Guid,
	Uri,
	Enum,
	Array,
	Object
}

internal sealed class ConditionalInfo
{
	public required object ConditionGroup { get; init; }
	public List<ConditionalTrigger> Triggers { get; init; } = new();
	public List<PropertyConditionalConsequence> PropertyConsequences { get; init; } = new();
}

internal sealed class ConditionalTrigger
{
	public required ConditionalTriggerType Type { get; init; }
	public required string PropertyName { get; init; }
	public required string PropertySchemaName { get; init; }
	public string? ExpectedValue { get; init; }
	public double? NumericValue { get; init; }
	public bool IsExclusive { get; init; }
}

internal enum ConditionalTriggerType
{
	Equality,
	Minimum,
	Maximum,
	Enum
}

internal sealed class PropertyConditionalConsequence
{
	public required string PropertySchemaName { get; init; }
	public bool IsConditionallyRequired { get; init; }
	public bool IsConditionallyReadOnly { get; init; }
	public bool IsConditionallyWriteOnly { get; init; }
	public List<AttributeInfo> ConditionalAttributes { get; init; } = new();
}

