using System.Collections.Generic;
using System.Text.Json.Serialization;
//using Json.Schema.Tests.Serialization;
//using Json.Schema.Tests.Suite;

namespace Json.Schema.Tests;

//[JsonSerializable(typeof(TestCollection))]
//[JsonSerializable(typeof(List<TestCollection>))]
[JsonSerializable(typeof(System.Drawing.Point))]
//[JsonSerializable(typeof(DeserializationTests.Foo))]
//[JsonSerializable(typeof(DeserializationTests.FooWithSchema))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(EvaluationResults))]
//[JsonSerializable(typeof(GithubTests.Model791))]
//[JsonSerializable(typeof(GithubTests.Model791Undecorated))]
internal partial class TestSerializerContext : JsonSerializerContext;