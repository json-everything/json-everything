using System.Drawing;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema.Generation.Tests.Serialization;
using Json.Schema.Generation.Tests.SourceGeneration;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests;

[SetUpFixture]
internal static class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

	[OneTimeSetUp]
	public static void Setup()
	{
		SchemaGeneratorConfiguration.Default.DefaultDialect = MetaSchemas.Draft202012Id;
	}
}

[JsonSerializable(typeof(Point))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(EvaluationResults))]
[JsonSerializable(typeof(DeserializationTests.Foo))]
[JsonSerializable(typeof(DeserializationTests.FooWithSchema))]
[JsonSerializable(typeof(ArrayGenerationTests.EnumTest))]
[JsonSerializable(typeof(TestModels.SimplePerson))]
[JsonSerializable(typeof(TestModels.CamelCasePerson))]
[JsonSerializable(typeof(TestModels.PersonWithNullable))]
[JsonSerializable(typeof(TestModels.PersonWithRequired))]
[JsonSerializable(typeof(TestModels.Status))]
[JsonSerializable(typeof(TestModels.PersonWithEnum))]
[JsonSerializable(typeof(TestModels.PersonWithDescription))]
[JsonSerializable(typeof(TestModels.ProductWithCustomAttributes))]
[JsonSerializable(typeof(TestModels.Address))]
[JsonSerializable(typeof(TestModels.PersonWithAddresses))]
[JsonSerializable(typeof(TestModels.SingleCondition))]
[JsonSerializable(typeof(TestModels.SingleConditionCamelCase))]
[JsonSerializable(typeof(TestModels.MultipleConditionGroups))]
[JsonSerializable(typeof(TestModels.MultipleTriggersInSameGroup))]
[JsonSerializable(typeof(TestModels.ConditionalWithMinimum))]
[JsonSerializable(typeof(TestModels.ConditionalWithMaximum))]
[JsonSerializable(typeof(TestModels.EnumSwitch))]
[JsonSerializable(typeof(TestModels.ConditionalValidation))]
[JsonSerializable(typeof(ClientTests.Issue977_RootType))]
[JsonSerializable(typeof(ClientTests.Issue977_ConfigTypeA))]
[JsonSerializable(typeof(ClientTests.Issue977_ConfigTypeB))]
[JsonSerializable(typeof(ClientTests.Issue977_SampleEnum))]
#if NET9_0_OR_GREATER
[JsonSerializable(typeof(ClientTests.Issue890_Status))]
#endif
public partial class TestSerializerContext : JsonSerializerContext;
