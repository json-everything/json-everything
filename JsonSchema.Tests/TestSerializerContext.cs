using System.Collections.Generic;
using System.Text.Json.Serialization;
using Json.Schema.Tests.Serialization;
using Json.Schema.Tests.Suite;

namespace Json.Schema.Tests;

[JsonSerializable(typeof(TestCollection))]
[JsonSerializable(typeof(List<TestCollection>))]
[JsonSerializable(typeof(Suite.Experiments.TestCase), TypeInfoPropertyName = "ExperimentsTestCase")]
[JsonSerializable(typeof(Suite.Experiments.TestCollection), TypeInfoPropertyName = "ExperimentsTestCollection")]
[JsonSerializable(typeof(List<Suite.Experiments.TestCase>), TypeInfoPropertyName = "ListOfExperimentsTestCase")]
[JsonSerializable(typeof(List<Suite.Experiments.TestCollection>), TypeInfoPropertyName = "ListOfExperimentsTestCollection")]
[JsonSerializable(typeof(System.Drawing.Point))]
[JsonSerializable(typeof(DeserializationTests.Foo))]
[JsonSerializable(typeof(DeserializationTests.FooWithSchema))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(EvaluationResults))]
[JsonSerializable(typeof(Experiments.EvaluationResults), TypeInfoPropertyName = "ExperimentsEvaluationResults")]
internal partial class TestSerializerContext : JsonSerializerContext;