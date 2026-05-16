using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Schema.Generation.Serialization;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Json.Schema.Generation.Tests.SourceGeneration;

public static class TestModels
{
	[GenerateJsonSchema]
	public class SimplePerson
	{
		public string Name { get; set; } = string.Empty;
		public int Age { get; set; }
	}

	[GenerateJsonSchema(PropertyNaming = NamingConvention.CamelCase)]
	public class CamelCasePerson
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public int Age { get; set; }
	}

	[GenerateJsonSchema(PropertyNaming = NamingConvention.CamelCase)]
	public class CamelCaseWithNestedType
	{
		public NestedType Nested { get; set; } = new();
	}

	public class NestedType
	{
		public string ExternalId { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
	}

	[GenerateJsonSchema]
	public class PersonWithNullable
	{
		public string Name { get; set; } = string.Empty;
		public string? Email { get; set; }
		public int? Age { get; set; }
	}

	[GenerateJsonSchema]
	public class PersonWithRequired
	{
		public required string Name { get; set; }
		public int Age { get; set; }
	}

	public enum Status
	{
		Active,
		Inactive,
		Pending
	}

	public enum ContentStyle
	{
		Confident,
		Passionate,
		Engaging,
		Practical,
		Humorous
	}

	[GenerateJsonSchema]
	public class PersonWithEnum
	{
		public string Name { get; set; } = string.Empty;
		public Status Status { get; set; }
	}

	[GenerateJsonSchema(PropertyNaming = NamingConvention.CamelCase)]
	public class ModelWithNullableEnumArray
	{
		public ContentStyle[]? Styles { get; set; }
	}

	[GenerateJsonSchema]
	public class PersonWithDescription
	{
		/// <summary>
		/// The person's full name
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// The person's age in years
		/// </summary>
		public int Age { get; set; }
	}

	[GenerateJsonSchema]
	public class ProductWithCustomAttributes
	{
		public string Name { get; set; } = string.Empty;

		[PositiveNumber]
		public decimal Price { get; set; }

		[Range(0, 100)]
		public int DiscountPercentage { get; set; }

		public string? Description { get; set; }
	}

	[GenerateJsonSchema(PropertyNaming = NamingConvention.CamelCase)]
	public class ModelWithGuidArrayAndMinItems
	{
		[Required]
		[MinItems(1)]
		public Guid[] RecipientIds { get; set; } = [];
	}

	public class Address
	{
		public string Street { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string PostalCode { get; set; } = string.Empty;
	}

	[GenerateJsonSchema]
	public class PersonWithAddresses
	{
		public string Name { get; set; } = string.Empty;
		[Description("Home address")]
		public Address? HomeAddress { get; set; }
		[Description("Work address")]
		public Address? WorkAddress { get; set; }
	}

	[GenerateJsonSchema]
	[If(nameof(Toggle), true, 0)]
	public class SingleCondition
	{
		[Required]
		public bool Toggle { get; set; }

		[Required(ConditionGroup = 0)]
		public string? Required { get; set; }
	}

	[GenerateJsonSchema(PropertyNaming = NamingConvention.CamelCase)]
	[If(nameof(Toggle), true, 0)]
	public class SingleConditionCamelCase
	{
		[Required]
		public bool Toggle { get; set; }

		[Required(ConditionGroup = 0)]
		public string? Required { get; set; }
	}

	[GenerateJsonSchema]
	[If(nameof(Toggle), true, "ifToggle")]
	[If(nameof(OtherToggle), 42, "ifOtherToggle")]
	public class MultipleConditionGroups
	{
		[Required]
		public bool Toggle { get; set; }

		[Required]
		public int OtherToggle { get; set; }

		[Required(ConditionGroup = "ifToggle")]
		public string? RequiredIfToggle { get; set; }

		[Required(ConditionGroup = "ifOtherToggle")]
		public string? RequiredIfOtherToggle { get; set; }
	}

	[GenerateJsonSchema]
	[If(nameof(Count), 1, 0)]
	[If(nameof(Name), "special", 0)]
	public class MultipleTriggersInSameGroup
	{
		[Required]
		public int Count { get; set; }

		[Required]
		public string Name { get; set; } = string.Empty;

		[Required(ConditionGroup = 0)]
		public string? SpecialField { get; set; }
	}

	[GenerateJsonSchema]
	[If(nameof(Status), nameof(TestModels.Status.Active), "state-note")]
	[If(nameof(Status), nameof(TestModels.Status.Pending), "state-note")]
	public class MultipleIfsSamePropertyAndGroup
	{
		[Required]
		public Status Status { get; set; }

		[Required(ConditionGroup = "state-note")]
		public string? Note { get; set; }
	}

	[GenerateJsonSchema]
	[If(nameof(Status), nameof(TestModels.Status.Active), "job-required")]
	[If(nameof(Status), nameof(TestModels.Status.Pending), "job-required")]
	[If(nameof(Status), nameof(TestModels.Status.Inactive), "org-required")]
	public class MultipleIfsSamePropertyAcrossGroups
	{
		[Required]
		public Status Status { get; set; }

		[Required(ConditionGroup = "job-required")]
		public Guid? JobListingId { get; set; }

		[Required(ConditionGroup = "org-required")]
		public Guid? OrgId { get; set; }
	}

	[GenerateJsonSchema]
	[IfMin(nameof(Age), 18, 0)]
	public class ConditionalWithMinimum
	{
		[Required]
		public int Age { get; set; }

		[Required(ConditionGroup = 0)]
		public string? AdultField { get; set; }
	}

	[GenerateJsonSchema]
	[IfMax(nameof(Score), 100, 0)]
	public class ConditionalWithMaximum
	{
		[Required]
		public int Score { get; set; }

		[Required(ConditionGroup = 0)]
		public string? BonusEligible { get; set; }
	}

	[GenerateJsonSchema]
	[IfEnum(nameof(Day))]
	public class EnumSwitch
	{
		[Required]
		public DayOfWeek Day { get; set; }

		[Required(ConditionGroup = DayOfWeek.Monday)]
		public string? MondayField { get; set; }

		[Required(ConditionGroup = DayOfWeek.Tuesday)]
		public string? TuesdayField { get; set; }
	}

	[GenerateJsonSchema]
	[If(nameof(IsActive), true, 0)]
	public class ConditionalValidation
	{
		[Required]
		public bool IsActive { get; set; }

		[MinLength(5, ConditionGroup = 0)]
		[MaxLength(100, ConditionGroup = 0)]
		public string? Name { get; set; }

		[Minimum(0, ConditionGroup = 0)]
		[Maximum(150, ConditionGroup = 0)]
		public int? Age { get; set; }
	}

	[GenerateJsonSchema(PropertyOrder = PropertyOrder.ByName)]
	public class PersonWithSortedProperties
	{
		public string Name { get; set; } = string.Empty;
		public int Age { get; set; }
		public string Email { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
	}

	[GenerateJsonSchema]
	[Id("https://json-everything.test/schemas/person")]
	public class PersonWithId
	{
		public string Name { get; set; } = string.Empty;
		public int Age { get; set; }
	}

	[GenerateJsonSchema]
	public class PersonWithIdReference
	{
		public string Name { get; set; } = string.Empty;
		public PersonWithId? Person { get; set; }
	}

	[GenerateJsonSchema]
	public class PersonWithJsonRequired
	{
		public string Name { get; set; } = string.Empty;
		[System.Text.Json.Serialization.JsonRequired]
		public int Age { get; set; }
	}

	[GenerateJsonSchema]
	public class PersonWithDefaults
	{
		[Default("anonymous")]
		public string Name { get; set; } = string.Empty;
		[Default(0)]
		public int Age { get; set; }
		[Default(true)]
		public bool IsActive { get; set; }
	}

	[GenerateJsonSchema]
	[AdditionalProperties(false)]
	public class PersonWithNoAdditionalProperties
	{
		public string Name { get; set; } = string.Empty;
		public int Age { get; set; }
	}

	public class GenericHolder<T>
	{
		public T Value { get; set; } = default!;
	}

	public class Optional<T>
	{
		public T Value { get; set; } = default!;
	}

	[GenerateJsonSchema]
	public class ModelWithMultipleClosedGenerics
	{
		public GenericHolder<int>? IntHolder { get; set; }
		public GenericHolder<string>? StringHolder { get; set; }
	}

	[GenerateJsonSchema]
	public class ModelWithOptionalWrapper
	{
		public Optional<int> Age { get; set; } = new();
	}

	[GenerateJsonSchema]
	public class ModelWithOptionalObjectWrapper
	{
		public Optional<SimplePerson> Person { get; set; } = new();
	}

	[GenerateJsonSchema]
	public class ModelWithNullableOptionalGuid
	{
		public Optional<Guid> ValueA { get; set; } = new();
		public Optional<Guid?> ValueB { get; set; } = new();
	}

	[GenerateJsonSchema]
	public class ModelWithOptionalCollections
	{
		public Optional<IEnumerable<int>> ValueA { get; set; } = new();
		public Optional<int[]> ValueB { get; set; } = new();
	}

	[GenerateJsonSchema]
	public class ModelWithOptionalUngeneratedType
	{
		public Optional<UngeneratedType> Ungenerated { get; set; } = new();
	}

	[GenerateJsonSchema]
	public class ModelWithOptionalDictionary
	{
		public Optional<Dictionary<string, int>> Data { get; set; } = new();
	}

	[GenerateJsonSchema]
	public class ModelWithOptionalAdditionalCollections
	{
		public Optional<HashSet<int>> ValueA { get; set; } = new();
		public Optional<Queue<int>> ValueB { get; set; } = new();
		public Optional<IReadOnlyCollection<int>> ValueC { get; set; } = new();
	}

	public class UngeneratedType
	{
		public string Foo { get; set; } = null!;
	}

	[GenerateJsonSchema]
	public class ModelWithBuiltInJsonTypes
	{
		public JsonDocument Document { get; set; } = null!;
		public JsonElement Element { get; set; }
		public JsonNode Node { get; set; } = null!;
		public JsonValue Value { get; set; } = JsonValue.Create(0)!;
		public JsonObject Object { get; set; } = new();
		public JsonArray Array { get; set; } = [];
	}

	[GenerateJsonSchema]
	public class ModelWithStringKeyDictionary
	{
		public Dictionary<string, int> Data { get; set; } = [];
	}

	[GenerateJsonSchema]
	public class ModelWithEnumKeyDictionary
	{
		public Dictionary<Status, bool> Flags { get; set; } = [];
	}

	[GenerateJsonSchema]
	public class ModelWithGuidKeyDictionary
	{
		public Dictionary<Guid, int> Data { get; set; } = [];
	}

	[GenerateJsonSchema]
	public class ConflictModel
	{
		public string Value { get; set; } = string.Empty;
	}

	/// <summary>
	/// First line of summary.
	/// Second line of summary.
	/// </summary>
	[GenerateJsonSchema]
	public class TypeWithMultiLineSummary
	{
		/// <summary>
		/// First line of property summary.
		/// Second line of property summary.
		/// </summary>
		public string Value { get; set; } = string.Empty;
	}

	[GenerateJsonSchema]
	public class ModelWithNullableOverrides
	{
		/// <summary>Force-nullable non-nullable string.</summary>
		[Nullable(true)]
		public string ForcedNullable { get; set; } = string.Empty;

		/// <summary>Force-non-nullable nullable int.</summary>
		[Nullable(false)]
		public int? ForcedNonNullable { get; set; }
	}

	[GenerateJsonSchema(PropertyNaming = NamingConvention.CamelCase)]
	public class ModelWithDuplicateSchemaPropertyNames
	{
		public int Foo { get; set; }
		public int foo { get; set; }
	}

	[GenerateJsonSchema(StrictConditionals = true)]
	[If(nameof(IsActive), true, 0)]
	public class StrictConditionalValidation
	{
		[Required]
		public bool IsActive { get; set; }

		[MinLength(5, ConditionGroup = 0)]
		[MaxLength(100, ConditionGroup = 0)]
		public string? Name { get; set; }

		[Minimum(0, ConditionGroup = 0)]
		[Maximum(150, ConditionGroup = 0)]
		public int? Age { get; set; }
	}

	public enum SourceGenTargetEnumeration
	{
		[JsonIgnore]
		IgnoreThis,

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		DontIgnoreThis,

		[JsonExclude]
		IgnoreThisWithJsonExclude,

		[JsonIgnore]
		[JsonExclude]
		IgnoreAndExcludeThis,

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		[JsonExclude]
		ExcludeTrumpsIgnore,

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
		[JsonExclude]
		ExcludeTrumpsIgnoreWhenWritingDefault,

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
		DontIgnoreThisWhenWritingDefault,

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		DontIgnoreThisWhenWritingNull,
	}

	[GenerateJsonSchema]
	public class SourceGenTarget
	{
		[JsonInclude]
#pragma warning disable CS0169
		private int _value;

		private int _notIncluded;
#pragma warning restore CS0169

		public SourceGenTargetEnumeration EnumProp { get; set; }

		[Required]
		[Minimum(5)]
		[ExclusiveMinimum(4)]
		[Maximum(10)]
		[ExclusiveMaximum(11)]
		[MultipleOf(1.5)]
		public int Integer { get; set; }

		[MaxLength(10)]
		[Pattern("^[a-z0-9_]$")]
		public string String { get; set; } = string.Empty;

		[Required]
		public string RequiredString { get; set; } = string.Empty;

		[JsonPropertyName("rename-this-required-string")]
		[Required]
		public string RenameThisRequiredString { get; set; } = string.Empty;

		[MinItems(5)]
		[MaxItems(10)]
		public List<bool> ListOfBool { get; set; }

		[MinLength(5, GenericParameter = 0)]
		[UniqueItems(true)]
		[Obsolete]
		public List<string> ListOfString { get; set; }

		[Maximum(100)]
		public int Duplicated1 { get; set; }

		[Maximum(100)]
		public int Duplicated2 { get; set; }

		public SourceGenTarget Target { get; set; }

		[JsonIgnore]
		public int IgnoreThis { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public string DontIgnoreThis { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
		public string DontIgnoreThisWhenWritingDefault { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string DontIgnoreThisWhenWritingNull { get; set; }

		[JsonExclude]
		public string IgnoreThisWithJsonExclude { get; set; }

		[JsonIgnore]
		[JsonExclude]
		public double IgnoreAndExcludeThis { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		[JsonExclude]
		public double ExcludeTrumpsIgnore { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
		[JsonExclude]
		public double ExcludeTrumpsIgnoreWhenWritingDefault { get; set; }

		[JsonPropertyName("rename-this")]
		public string RenameThis { get; set; }

		public float StrictNumber { get; set; }
		public float OtherStrictNumber { get; set; }

		[ReadOnly]
		public float ReadOnlyNumber { get; set; }

		[WriteOnly]
		public float WriteOnlyNumber { get; set; }

		[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
		public float StringyNumber { get; set; }

		[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
		public float NotANumber { get; set; }

		[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals)]
		public float StringyNotANumber { get; set; }

		[Title("title")]
		[Description("description")]
		public string Metadata { get; set; }
	}
}
