using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using TestHelpers;

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
                public string? Property { get; set; }
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
		var builder = new JsonSchemaBuilder();
		builder.FromType(type);

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var typeValue = valueType switch
		{
			SchemaValueType.String => "\"string\"",
			SchemaValueType.Integer => "\"integer\"",
			SchemaValueType.String | SchemaValueType.Null => "[\"null\", \"string\"]",
			SchemaValueType.Integer | SchemaValueType.Null => "[\"null\", \"integer\"]",
			_ => throw new ArgumentOutOfRangeException(nameof(valueType))
		};

		var expected = JsonDocument.Parse(
			$$"""
			{
			  "type": "object",
			  "properties": {
			    "Property": {"type": {{typeValue}}}
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
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
		var builder = new JsonSchemaBuilder();
		builder.FromType(type);

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var enumValues = string.Join(", ", Enum.GetNames(typeof(DayOfWeek)).Select(v => $"\"{v}\""));
		if (containsNull)
			enumValues += ", null";

		var expected = JsonDocument.Parse(
			$$"""
			{
			  "type": "object",
			  "properties": {
			    "Property": {"enum": [{{enumValues}}]}
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
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
			.FromType<DifferingNullabilityValueType>(new SchemaGeneratorConfiguration())
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
			.FromType<DifferingNullabilityValueTypeUsingAttribute>(new SchemaGeneratorConfiguration())
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
			.FromType<DifferingNullabilityReferenceType>(new SchemaGeneratorConfiguration())
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