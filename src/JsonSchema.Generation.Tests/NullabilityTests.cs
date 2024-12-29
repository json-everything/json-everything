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

	public class ReferenceMemberNullable
	{
		public string? Property { get; set; }
	}

	public class ReferenceMemberWithNull
	{
		[Nullable(true)]
		public string Property { get; set; }
	}

	public class ReferenceMemberNullableWithNull
	{
		[Nullable(true)]
		public string? Property { get; set; }
	}

	public class ReferenceMemberWithNotNull
	{
		[Nullable(false)]
		public string Property { get; set; }
	}

	public class ReferenceMemberNullableWithNotNull
	{
		[Nullable(false)]
		public string Property { get; set; }
	}

	public class ValueTypeMember
	{
		public int Property { get; set; }
	}

	public class ValueTypeMemberNullable
	{
		public int? Property { get; set; }
	}

	public class ValueTypeMemberWithNull
	{
		[Nullable(true)]
		public int Property { get; set; }
	}

	public class ValueTypeMemberNullableWithNull
	{
		[Nullable(true)]
		public int? Property { get; set; }
	}

	public class ValueTypeMemberNullableWithNotNull
	{
		[Nullable(false)]
		public int? Property { get; set; }
	}

	public class ValueTypeMemberWithNotNull
	{
		[Nullable(false)]
		public int Property { get; set; }
	}

	private static readonly SchemaValueType String = SchemaValueType.String;
	private static readonly SchemaValueType Integer = SchemaValueType.Integer;
	private static readonly SchemaValueType Null = SchemaValueType.Null;


	public static IEnumerable<TestCaseData> MemberCases
	{
		get
		{
			yield return new TestCaseData(typeof(ReferenceMember), String);
			yield return new TestCaseData(typeof(ReferenceMemberWithNull), String | Null);
			yield return new TestCaseData(typeof(ReferenceMemberWithNotNull), String);
			yield return new TestCaseData(typeof(ReferenceMemberNullable), String | Null);
			yield return new TestCaseData(typeof(ReferenceMemberNullableWithNull), String | Null);
			yield return new TestCaseData(typeof(ReferenceMemberNullableWithNotNull), String);

			yield return new TestCaseData(typeof(ValueTypeMember), Integer);
			yield return new TestCaseData(typeof(ValueTypeMemberWithNull), Integer | Null);
			yield return new TestCaseData(typeof(ValueTypeMemberWithNotNull), Integer);
			yield return new TestCaseData(typeof(ValueTypeMemberNullable), Integer | Null);
			yield return new TestCaseData(typeof(ValueTypeMemberNullableWithNull), Integer | Null);
			yield return new TestCaseData(typeof(ValueTypeMemberNullableWithNotNull), Integer);
		}
	}

	[TestCaseSource(nameof(MemberCases))]
	public void MemberNullability(Type type, SchemaValueType valueType)
	{
		// Nullability affects root schema so only PropertiesKeywords are compared
		var expected = new JsonSchemaBuilder()
			.Properties(
				(nameof(ReferenceMember.Property), new JsonSchemaBuilder().Type(valueType)))
			.Build()
			.Keywords!
			.OfType<PropertiesKeyword>()
			.First();

		var actual = new JsonSchemaBuilder()
			.FromType(type)
			.Build()
			.Keywords!
			.OfType<PropertiesKeyword>()
			.First();

		AssertEqual(expected, actual);
	}

	public class EnumMember
	{
		public DayOfWeek Property { get; set; }
	}

	public class EnumMemberNullable
	{
		public DayOfWeek? Property { get; set; }
	}

	public class EnumMemberWithNull
	{
		[Nullable(true)]
		public DayOfWeek Property { get; set; }
	}

	public class EnumMemberNullableWithNull
	{
		[Nullable(true)]
		public DayOfWeek? Property { get; set; }
	}

	public class EnumMemberWithNotNull
	{
		[Nullable(false)]
		public DayOfWeek Property { get; set; }
	}

	public class EnumMemberNullableWithNotNull
	{
		[Nullable(false)]
		public DayOfWeek? Property { get; set; }
	}

	public static IEnumerable<TestCaseData> EnumMemberCases
	{
		get
		{
			yield return new TestCaseData(typeof(EnumMember), false);
			yield return new TestCaseData(typeof(EnumMemberWithNull), true);
			yield return new TestCaseData(typeof(EnumMemberWithNotNull), false);
			yield return new TestCaseData(typeof(EnumMemberNullable), true);
			yield return new TestCaseData(typeof(EnumMemberNullableWithNull), true);
			yield return new TestCaseData(typeof(EnumMemberNullableWithNotNull), false);
		}
	}

	[TestCaseSource(nameof(EnumMemberCases))]
	public void EnumMemberNullability(Type type, bool containsNull)
	{
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
			.FromType(type)
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
			yield return new TestCaseData(typeof(string), String);
			yield return new TestCaseData(typeof(int?), Integer);
			yield return new TestCaseData(typeof(int), Integer);
		}
	}

	[TestCaseSource(nameof(TypeCases))]
	public void TypeNullability(Type type, SchemaValueType valueType)
	{
		var expected = new JsonSchemaBuilder()
			.Type(valueType)
			.Build();

		var actual = new JsonSchemaBuilder()
			.FromType(type)
			.Build();

		AssertEqual(expected, actual);
	}

	public static IEnumerable<TestCaseData> EnumTypeCases
	{
		get
		{
			yield return new TestCaseData(typeof(DayOfWeek), false);
			yield return new TestCaseData(typeof(DayOfWeek?), false);
		}
	}

	[TestCaseSource(nameof(EnumTypeCases))]
	public void EnumTypeNullability(Type type, bool containsNull)
	{
		var values = Enum.GetNames(typeof(DayOfWeek)).ToList();
		if (containsNull)
			values.Add(null!);

		var expected = new JsonSchemaBuilder()
			.Enum(values)
			.Build();

		var actual = new JsonSchemaBuilder()
			.FromType(type)
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
				Optimize = false
			})
			.Build();

		AssertEqual(expected, actual);
	}

	private class DifferingNullabilityValueTypeUsingAttribute
	{
		public int NonNullable { get; set; }
		[Nullable(true)]
		public int Nullable { get; set; }
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
		public string Nullable { get; set; }
		public string? AlsoNullable { get; set; }
		[Nullable(false)]
		public string? OverriddenNotNullable { get; set; }
	}

	[Test]
	public void NullableStringAndNonNullableString()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("NonNullable", new JsonSchemaBuilder().Type(String)),
				("Nullable", new JsonSchemaBuilder().Type(String | Null)),
				("AlsoNullable", new JsonSchemaBuilder().Type(String | Null)),
				("OverriddenNotNullable", new JsonSchemaBuilder().Type(String))
			);

		var actual = new JsonSchemaBuilder()
			.FromType<DifferingNullabilityReferenceType>(new SchemaGeneratorConfiguration
			{
				Optimize = false
			})
			.Build();

		AssertEqual(expected, actual);
	}

	private class NullableDateTime
	{
		public DateTime? Property { get; set; }
	}

	[Test]
	public void NullableStruct()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Property", new JsonSchemaBuilder()
					.Type(SchemaValueType.String | SchemaValueType.Null)
					.Format(Formats.DateTime)
				)
			);

		var actual = new JsonSchemaBuilder()
			.FromType<NullableDateTime>()
			.Build();

		AssertEqual(expected, actual);
	}

	private class NullableDateTimeWithDescription
	{
		[Description("description")]
		public DateTime? Property { get; set; }
	}

	[Test]
	public void NullableStructWithAnotherAttribute()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Property", new JsonSchemaBuilder()
					.Type(SchemaValueType.String | SchemaValueType.Null)
					.Format(Formats.DateTime)
					.Description("description")
				)
			);

		var actual = new JsonSchemaBuilder()
			.FromType<NullableDateTimeWithDescription>()
			.Build();

		AssertEqual(expected, actual);
	}
}