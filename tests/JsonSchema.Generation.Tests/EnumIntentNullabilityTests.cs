using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema.Generation.Generators;
using Json.Schema.Generation.Intents;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Json.Schema.Generation.Tests;

/// <summary>
/// Regression tests for making an enum nullable when the <see cref="EnumIntent"/>
/// was created from values instead of names (issue #951).
/// </summary>
public class EnumIntentNullabilityTests
{
	public enum NumericEnum
	{
		One = 1,
		Two = 2
	}

	/// <summary>
	/// Emits an <see cref="EnumIntent"/> built from values only, mirroring generators
	/// that emit numeric enum values instead of member names.
	/// </summary>
	private class ValueBasedEnumGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(NumericEnum);
		}

		public void AddConstraints(SchemaGenerationContextBase context)
		{
			context.Intents.Add(new EnumIntent(new JsonNode[]
			{
				JsonValue.Create(1)!,
				JsonValue.Create(2)!
			}));
		}
	}

	private class NullableValueEnumMember
	{
		public NumericEnum? Property { get; set; }
	}

	[Test]
	public void NullableEnumBuiltFromValuesDoesNotThrow()
	{
		var configuration = new SchemaGeneratorConfiguration
		{
			DefaultDialect = SchemaGeneratorConfiguration.Default.DefaultDialect
		};
		configuration.Generators.Insert(0, new ValueBasedEnumGenerator());

		JsonSchema expected = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Type(SchemaValueType.Object)
			.Properties(
				("Property", new JsonSchemaBuilder()
					.Enum(new JsonNode?[] { 1, 2, null })
				)
			);

		VerifyGeneration<NullableValueEnumMember>(expected, configuration);
	}
}
