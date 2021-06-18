using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Tests
{
	public class NullabilityTests
	{
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
				.Keywords
				.OfType<PropertiesKeyword>()
				.First();

			var actual = new JsonSchemaBuilder()
				.FromType(type, config)
				.Build()
				.Keywords
				.OfType<PropertiesKeyword>()
				.First();

			Assert.AreEqual(expected, actual);
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

			Assert.AreEqual(expected, actual);
		}

	}
}
