using System.Collections.Generic;
using System.Text.Json.Serialization;
using Json.Schema.Tests.Serialization;
using Json.Schema.Tests.Suite;

namespace Json.Schema.Tests;

[JsonSerializable(typeof(TestCollection))]
[JsonSerializable(typeof(List<TestCollection>))]
[JsonSerializable(typeof(System.Drawing.Point))]
[JsonSerializable(typeof(DeserializationTests.Foo))]
[JsonSerializable(typeof(DeserializationTests.FooWithSchema))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(EvaluationResults))]
[JsonSerializable(typeof(GithubTests.Model791))]
[JsonSerializable(typeof(GithubTests.Model791Undecorated))]
#if DEBUG
[JsonSerializable(typeof(Suite.Experiments.TestCase), TypeInfoPropertyName = "ExperimentsTestCase")]
[JsonSerializable(typeof(List<Suite.Experiments.TestCase>), TypeInfoPropertyName = "ExperimentsTestCaseList")]
[JsonSerializable(typeof(Suite.Experiments.TestCollection), TypeInfoPropertyName = "ExperimentsTestCollection")]
[JsonSerializable(typeof(List<Suite.Experiments.TestCollection>), TypeInfoPropertyName = "ExperimentsTestCollectionList")]
[JsonSerializable(typeof(Experiments.EvaluationResults), TypeInfoPropertyName = "ExperimentsEvaluationResults")]
internal partial class TestSerializerContext : JsonSerializerContext;