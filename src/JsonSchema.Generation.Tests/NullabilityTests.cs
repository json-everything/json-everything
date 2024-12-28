using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming

namespace Json.Schema.Generation.Tests;

public class NullabilityTests
{
	// ReSharper disable UnusedMember.Global
	// ReSharper disable MemberCanBePrivate.Global
	public class ReferenceMember
	{
		public string Property { get; set; }
	}

	public class ReferenceMemberWithNull
	{
		[Nullable(true)]
		public string Property { get; set; }
	}

	public class ReferenceMemberWithNotNull
	{
		[Nullable(false)]
		public string Property { get; set; }
	}

	public class NullableValueTypeMember
	{
		public int? Property { get; set; }
	}

	public class NullableValueTypeMemberWithNull
	{
		[Nullable(true)]
		public int? Property { get; set; }
	}

	public class NullableValueTypeMemberWithNotNull
	{
		[Nullable(false)]
		public int? Property { get; set; }
	}

	public class ValueTypeMember
	{
		public int Property { get; set; }
	}

	public class ValueTypeMemberWithNull
	{
		[Nullable(true)]
		public int Property { get; set; }
	}

	public class ValueTypeMemberWithNotNull
	{
		[Nullable(false)]
		public int Property { get; set; }
	}

	public class NullableEnumMember
	{
		public DayOfWeek? Property { get; set; }
	}

	public class NullableEnumMemberWithNull
	{
		[Nullable(true)]
		public DayOfWeek? Property { get; set; }
	}

	public class NullableEnumMemberWithNotNull
	{
		[Nullable(false)]
		public DayOfWeek? Property { get; set; }
	}

	public class EnumMember
	{
		public DayOfWeek Property { get; set; }
	}

	public class EnumMemberWithNull
	{
		[Nullable(true)]
		public DayOfWeek Property { get; set; }
	}

	public class EnumMemberWithNotNull
	{
		[Nullable(false)]
		public DayOfWeek Property { get; set; }
	}

	private static readonly Nullability Disabled = Nullability.Disabled;
	private static readonly Nullability AllowForReferenceTypes = Nullability.AllowForReferenceTypes;
	private static readonly Nullability AllowForNullableValueTypes = Nullability.AllowForNullableValueTypes;
	private static readonly Nullability AllowForAllTypes = Nullability.AllowForAllTypes;
	private static readonly SchemaValueType String = SchemaValueType.String;
	private static readonly SchemaValueType Integer = SchemaValueType.Integer;
	private static readonly SchemaValueType Null = SchemaValueType.Null;


	public static IEnumerable<TestCaseData> MemberCases
	{
		get
		{
			yield return new TestCaseData(Disabled, typeof(ReferenceMember), String);
			yield return new TestCaseData(Disabled, typeof(ReferenceMemberWithNull), String | Null);
			yield return new TestCaseData(Disabled, typeof(ReferenceMemberWithNotNull), String);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(ReferenceMember), String);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(ReferenceMemberWithNull), String | Null);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(ReferenceMemberWithNotNull), String);
			yield return new TestCaseData(AllowForAllTypes, typeof(ReferenceMember), String | Null);
			yield return new TestCaseData(AllowForAllTypes, typeof(ReferenceMemberWithNull), String | Null);
			yield return new TestCaseData(AllowForAllTypes, typeof(ReferenceMemberWithNotNull), String);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(ReferenceMember), String | Null);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(ReferenceMemberWithNull), String | Null);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(ReferenceMemberWithNotNull), String);

			yield return new TestCaseData(Disabled, typeof(NullableValueTypeMember), Integer);
			yield return new TestCaseData(Disabled, typeof(NullableValueTypeMemberWithNull), Integer | Null);
			yield return new TestCaseData(Disabled, typeof(NullableValueTypeMemberWithNotNull), Integer);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(NullableValueTypeMember), Integer | Null);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(NullableValueTypeMemberWithNull), Integer | Null);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(NullableValueTypeMemberWithNotNull), Integer);
			yield return new TestCaseData(AllowForAllTypes, typeof(NullableValueTypeMember), Integer | Null);
			yield return new TestCaseData(AllowForAllTypes, typeof(NullableValueTypeMemberWithNull), Integer | Null);
			yield return new TestCaseData(AllowForAllTypes, typeof(NullableValueTypeMemberWithNotNull), Integer);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(NullableValueTypeMember), Integer);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(NullableValueTypeMemberWithNull), Integer | Null);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(NullableValueTypeMemberWithNotNull), Integer);

			yield return new TestCaseData(Disabled, typeof(ValueTypeMember), Integer);
			yield return new TestCaseData(Disabled, typeof(ValueTypeMemberWithNull), Integer | Null);
			yield return new TestCaseData(Disabled, typeof(ValueTypeMemberWithNotNull), Integer);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(ValueTypeMember), Integer);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(ValueTypeMemberWithNull), Integer | Null);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(ValueTypeMemberWithNotNull), Integer);
			yield return new TestCaseData(AllowForAllTypes, typeof(ValueTypeMember), Integer);
			yield return new TestCaseData(AllowForAllTypes, typeof(ValueTypeMemberWithNull), Integer | Null);
			yield return new TestCaseData(AllowForAllTypes, typeof(ValueTypeMemberWithNotNull), Integer);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(ValueTypeMember), Integer);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(ValueTypeMemberWithNull), Integer | Null);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(ValueTypeMemberWithNotNull), Integer);
		}
	}

	[TestCaseSource(nameof(MemberCases))]
	public void MemberNullability(Nullability nullability, Type type, SchemaValueType valueType)
	{
		var config = new SchemaGeneratorConfiguration
		{
			Nullability = nullability
		};

		// Nullability affects root schema so only PropertiesKeywords are compared
		var expected = new JsonSchemaBuilder()
			.Properties(
				(nameof(ReferenceMember.Property), new JsonSchemaBuilder().Type(valueType)))
			.Build()
			.Keywords!
			.OfType<PropertiesKeyword>()
			.First();

		var actual = new JsonSchemaBuilder()
			.FromType(type, config)
			.Build()
			.Keywords!
			.OfType<PropertiesKeyword>()
			.First();

		AssertEqual(expected, actual);
	}

	public static IEnumerable<TestCaseData> EnumMemberCases
	{
		get
		{
			yield return new TestCaseData(Disabled, typeof(EnumMember), false);
			yield return new TestCaseData(Disabled, typeof(EnumMemberWithNull), true);
			yield return new TestCaseData(Disabled, typeof(EnumMemberWithNotNull), false);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(EnumMember), false);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(EnumMemberWithNull), true);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(EnumMemberWithNotNull), false);
			yield return new TestCaseData(AllowForAllTypes, typeof(EnumMember), false);
			yield return new TestCaseData(AllowForAllTypes, typeof(EnumMemberWithNull), true);
			yield return new TestCaseData(AllowForAllTypes, typeof(EnumMemberWithNotNull), false);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(EnumMember), false);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(EnumMemberWithNull), true);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(EnumMemberWithNotNull), false);
		}
	}

	[TestCaseSource(nameof(EnumMemberCases))]
	public void EnumMemberNullability(Nullability nullability, Type type, bool containsNull)
	{
		var config = new SchemaGeneratorConfiguration
		{
			Nullability = nullability
		};

		var values = Enum.GetNames(typeof(DayOfWeek)).ToList();
		if (containsNull)
			values.Add(null!);
		// Nullability affects root schema so only PropertiesKeywords are compared
		var expected = new JsonSchemaBuilder()
			.Properties(
				(nameof(ReferenceMember.Property), new JsonSchemaBuilder().Enum(values)))
			.Build()
			.Keywords!
			.OfType<PropertiesKeyword>()
			.First();

		var actual = new JsonSchemaBuilder()
			.FromType(type, config)
			.Build()
			.Keywords!
			.OfType<PropertiesKeyword>()
			.First();

		AssertEqual(expected, actual);
	}

	public static IEnumerable<TestCaseData> TypeCases
	{
		get
		{
			yield return new TestCaseData(Disabled, typeof(string), String);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(string), String);
			yield return new TestCaseData(AllowForAllTypes, typeof(string), String | Null);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(string), String | Null);

			yield return new TestCaseData(Disabled, typeof(int?), Integer);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(int?), Integer | Null);
			yield return new TestCaseData(AllowForAllTypes, typeof(int?), Integer | Null);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(int?), Integer);

			yield return new TestCaseData(Disabled, typeof(int), Integer);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(int), Integer);
			yield return new TestCaseData(AllowForAllTypes, typeof(int), Integer);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(int), Integer);
		}
	}

	[TestCaseSource(nameof(TypeCases))]
	public void TypeNullability(Nullability nullability, Type type, SchemaValueType valueType)
	{
		var config = new SchemaGeneratorConfiguration
		{
			Nullability = nullability
		};

		var expected = new JsonSchemaBuilder()
			.Type(valueType)
			.Build();

		var actual = new JsonSchemaBuilder()
			.FromType(type, config)
			.Build();

		AssertEqual(expected, actual);
	}

	public static IEnumerable<TestCaseData> EnumTypeCases
	{
		get
		{
			yield return new TestCaseData(Disabled, typeof(DayOfWeek?), false);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(DayOfWeek?), true);
			yield return new TestCaseData(AllowForAllTypes, typeof(DayOfWeek?), true);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(DayOfWeek?), false);

			yield return new TestCaseData(Disabled, typeof(DayOfWeek), false);
			yield return new TestCaseData(AllowForNullableValueTypes, typeof(DayOfWeek), false);
			yield return new TestCaseData(AllowForAllTypes, typeof(DayOfWeek), false);
			yield return new TestCaseData(AllowForReferenceTypes, typeof(DayOfWeek), false);
		}
	}

	[TestCaseSource(nameof(EnumTypeCases))]
	public void EnumTypeNullability(Nullability nullability, Type type, bool containsNull)
	{
		var config = new SchemaGeneratorConfiguration
		{
			Nullability = nullability
		};

		var values = Enum.GetNames(typeof(DayOfWeek)).ToList();
		if (containsNull)
			values.Add(null!);

		var expected = new JsonSchemaBuilder()
			.Enum(values)
			.Build();

		var actual = new JsonSchemaBuilder()
			.FromType(type, config)
			.Build();

		AssertEqual(expected, actual);
	}

	private class DifferingNullabilityValueType
	{
		public int NonNullable { get; set; }
		public int? Nullable { get; set; }
	}

	[Test]
	public void NullableIntAndNonNullableInt()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("NonNullable", new JsonSchemaBuilder().Type(Integer)),
				("Nullable", new JsonSchemaBuilder().Type(Integer | Null))
			);

		var actual = new JsonSchemaBuilder()
			.FromType<DifferingNullabilityValueType>(new SchemaGeneratorConfiguration
			{
				Nullability = AllowForNullableValueTypes,
				Optimize = false
			})
			.Build();

		AssertEqual(expected, actual);
	}

	private class DifferingNullabilityValueTypeUsingAttribute
	{
		public int NonNullable { get; set; }
		[Nullable(true)]
		public int? Nullable { get; set; }
	}

	[Test]
	public void NullableIntAndNonNullableIntUsingAttribute()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("NonNullable", new JsonSchemaBuilder().Type(Integer)),
				("Nullable", new JsonSchemaBuilder().Type(Integer | Null))
			);

		var actual = new JsonSchemaBuilder()
			.FromType<DifferingNullabilityValueTypeUsingAttribute>(new SchemaGeneratorConfiguration
			{
				Optimize = false
			})
			.Build();

		AssertEqual(expected, actual);
	}

	private class DifferingNullabilityReferenceType
	{
		public string NonNullable { get; set; }
		[Nullable(true)]
		public string? Nullable { get; set; }
	}

	[Test]
	public void NullableStringAndNonNullableString()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("NonNullable", new JsonSchemaBuilder().Type(String)),
				("Nullable", new JsonSchemaBuilder().Type(String | Null))
			);

		var actual = new JsonSchemaBuilder()
			.FromType<DifferingNullabilityReferenceType>(new SchemaGeneratorConfiguration
			{
				Nullability = AllowForNullableValueTypes,
				Optimize = false
			})
			.Build();

		AssertEqual(expected, actual);
	}

	private class NullableDateTime
	{
		[Nullable(true)]
		public DateTime? Property { get; set; }
	}

	[Test]
	public void NullableStruct()
	{
		var expected = new JsonSchemaBuilder()
			.AnyOf(
				new JsonSchemaBuilder().Ref("#/$defs/dateTime"),
				new JsonSchemaBuilder().Type(SchemaValueType.Null)
			)
			.Definitions(
				("dateTime", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.DateTime)
				));

		var actual = new JsonSchemaBuilder()
			.FromType<NullableDateTime>()
			.Build();

		AssertEqual(expected, actual);
	}

	private class NullableDateTimeWithDescription
	{
		[Nullable(true)]
		[Description("description")]
		public DateTime? Property { get; set; }
	}

	[Test]
	public void NullableStructWithAnotherAttribute()
	{
		var expected = new JsonSchemaBuilder()
			.AnyOf(
				new JsonSchemaBuilder()
					.Ref("#/$defs/dateTime")
					.Description("description"),
				new JsonSchemaBuilder().Type(SchemaValueType.Null)
			)
			.Definitions(
				("dateTime", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.DateTime)
				));

		var actual = new JsonSchemaBuilder()
			.FromType<NullableDateTimeWithDescription>()
			.Build();

		AssertEqual(expected, actual);
	}
}