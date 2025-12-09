using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.Encodings.Web;
using System.Text.Json;
using Json.Schema.Serialization;
using NUnit.Framework;
using TestHelpers;
using RangeAttribute = System.ComponentModel.DataAnnotations.RangeAttribute;

namespace Json.Schema.Tests.Serialization;

public class DeserializationTests
{
	internal class Foo
	{
		[MinLength(5)]
		public string? Bar { get; set; }
		[Range(40, int.MaxValue)]
		[Required]
		public int Value { get; set; }
	}

	[JsonSchema(typeof(DeserializationTests), nameof(FooSchema))]
	internal class FooWithSchema
	{
		public string? Bar { get; set; }
		public int Value { get; set; }
	}

	public static readonly JsonSchema FooSchema =
		new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				(nameof(Foo.Bar), new JsonSchemaBuilder()
					.Type(SchemaValueType.String | SchemaValueType.Null)
					.MinLength(5)
				),
				(nameof(Foo.Value), new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(40)
				)
			)
			.Required(nameof(Foo.Value));

	public static readonly JsonSchema PointSchema =
		new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("X", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("Y", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.AdditionalProperties(false);

	private static readonly JsonSerializerOptions _options = new()
	{
		TypeInfoResolverChain = { TestSerializerContext.Default },
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		Converters = { new ValidatingJsonConverter { EvaluationOptions = { OutputFormat = OutputFormat.List } } }
	};

	[SetUp]
	public void Setup()
	{
		ValidatingJsonConverter.MapType<Point>(PointSchema);
	}

	/// <summary>
	/// Demonstrates that even without a schema, the incorrect JSON data type is
	/// caught by the serializer. However, the error is only thrown for the first
	/// problem that is encountered. The serializer doesn't report that `Value`
	/// is also incorrect.
	/// </summary>
	[Test]
	public void NoSchema_BadType()
	{
		Assert.Throws<JsonException>(() =>
		{
			try
			{
				var jsonText = @"{
  ""Bar"": 10,
  ""Value"": ""value""
}";

				var model = JsonSerializer.Deserialize<Foo>(jsonText, _options);

				TestConsole.WriteLine(JsonSerializer.Serialize(model, _options));
			}
			catch (Exception e)
			{
				HandleException(e);
				Assert.That(e.Message, Is.Not.EqualTo("JSON does not meet schema requirements"));
				throw;
			}
		});
	}

	/// <summary>
	/// Demonstrates that with a schema, the entire payload is validated and all of
	/// the problems are reported.
	/// </summary>
	[Test]
	public void WithSchema_BadType()
	{
		Assert.Throws<JsonException>(() =>
		{
			try
			{
				var jsonText = @"{
  ""Bar"": 10,
  ""Value"": ""value""
}";

				var model = JsonSerializer.Deserialize<FooWithSchema>(jsonText, _options);

				TestConsole.WriteLine(JsonSerializer.Serialize(model, _options));
			}
			catch (Exception e)
			{
				HandleException(e);
				Assert.That(e.Message, Is.EqualTo("JSON does not meet schema requirements"));
				throw;
			}
		});
	}

	/// <summary>
	/// Demonstrates that the serializer doesn't consider data annotations.
	/// As long as the data types are correct, the serializer will complete,
	/// even if the values are out of the acceptable range.
	/// </summary>
	[Test]
	public void NoSchema_BadValue()
	{
		try
		{
			var jsonText = @"{
""Bar"": ""bar"",
""Value"": 30
}";

			var model = JsonSerializer.Deserialize<Foo>(jsonText, _options);

			TestConsole.WriteLine(JsonSerializer.Serialize(model, _options));
		}
		catch (Exception e)
		{
			HandleException(e);
			throw;
		}
	}

	/// <summary>
	/// Demonstrates that the serializer doesn't consider required properties
	/// that are missing from the data.
	/// </summary>
	[Test]
	public void NoSchema_MissingValue()
	{
		try
		{
			var jsonText = @"{
""Bar"": ""bar""
}";

			var model = JsonSerializer.Deserialize<Foo>(jsonText, _options);

			TestConsole.WriteLine(JsonSerializer.Serialize(model, _options));
		}
		catch (Exception e)
		{
			HandleException(e);
			throw;
		}
	}

	/// <summary>
	/// Demonstrates that with a schema, out-of-range values are captured and
	/// deserialization stops.
	/// </summary>
	[Test]
	public void WithSchema_BadValue()
	{
		Assert.Throws<JsonException>(() =>
		{
			try
			{
				var jsonText = @"{
  ""Bar"": ""bar"",
  ""Value"": 30
}";

				var model = JsonSerializer.Deserialize<FooWithSchema>(jsonText, _options);

				TestConsole.WriteLine(JsonSerializer.Serialize(model, _options));
			}
			catch (Exception e)
			{
				HandleException(e);
				throw;
			}
		});
	}

	/// <summary>
	/// Demonstrates that with a schema, missing required values are captured
	/// and deserialization stops.
	/// </summary>
	[Test]
	public void WithSchema_MissingValue()
	{
		Assert.Throws<JsonException>(() =>
		{
			try
			{
				var jsonText = @"{
  ""Bar"": ""bar""
}";

				var model = JsonSerializer.Deserialize<FooWithSchema>(jsonText, _options);

				TestConsole.WriteLine(JsonSerializer.Serialize(model, _options));
			}
			catch (Exception e)
			{
				HandleException(e);
				throw;
			}
		});
	}

	/// <summary>
	/// Demonstrates a happy path without a schema.
	/// </summary>
	[Test]
	public void NoSchema_GoodValue()
	{
		try
		{
			var jsonText = @"{
  ""Bar"": ""bartholomew"",
  ""Value"": 42
}";

			var model = JsonSerializer.Deserialize<Foo>(jsonText, _options);

			TestConsole.WriteLine(JsonSerializer.Serialize(model, _options));
		}
		catch (Exception e)
		{
			HandleException(e);
			throw;
		}
	}

	/// <summary>
	/// Demonstrates a happy path with a schema.
	/// </summary>
	[Test]
	public void WithSchema_GoodValue()
	{
		try
		{
			var jsonText = @"{
  ""Bar"": ""bartholomew"",
  ""Value"": 42
}";

			var model = JsonSerializer.Deserialize<FooWithSchema>(jsonText, _options);

			TestConsole.WriteLine(JsonSerializer.Serialize(model, _options));
		}
		catch (Exception e)
		{
			HandleException(e);
			throw;
		}
	}

	[Test]
	public void WithSchema_MultipleDeserializations()
	{
		try
		{
			var jsonText = @"{
  ""Bar"": ""bartholomew"",
  ""Value"": 42
}";

			for (int i = 0; i < 10; i++)
			{
				var options = new JsonSerializerOptions
				{
					TypeInfoResolverChain = { TestSerializerContext.Default },
					WriteIndented = true,
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
					Converters = { new ValidatingJsonConverter { EvaluationOptions = { OutputFormat = OutputFormat.List } } }
				};

				var model = JsonSerializer.Deserialize<FooWithSchema>(jsonText, options);

				TestConsole.WriteLine(JsonSerializer.Serialize(model, options));
			}
		}
		catch (Exception e)
		{
			HandleException(e);
			throw;
		}
	}

	[Test]
	public void InaccessibleType_Valid()
	{
		try
		{
			var jsonText = @"{
  ""X"": 4,
  ""Y"": 5
}";

			var options = new JsonSerializerOptions
			{
				TypeInfoResolverChain = { TestSerializerContext.Default },
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				Converters = { new ValidatingJsonConverter { EvaluationOptions = { OutputFormat = OutputFormat.List } } }
			};

			var model = JsonSerializer.Deserialize<Point>(jsonText, options);

			TestConsole.WriteLine(JsonSerializer.Serialize(model, options));
		}
		catch (Exception e)
		{
			HandleException(e);
			throw;
		}
	}

	[Test]
	public void InaccessibleType_Invalid()
	{
		Assert.Throws<JsonException>(() =>
			{
				try
				{
					var jsonText = @"{
  ""X"": ""string"",
  ""Y"": 5
}";

					var options = new JsonSerializerOptions
					{
						TypeInfoResolverChain = { TestSerializerContext.Default },
						WriteIndented = true,
						Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
						Converters = { new ValidatingJsonConverter {EvaluationOptions = { OutputFormat = OutputFormat.List } } }
					};

					var model = JsonSerializer.Deserialize<Point>(jsonText, options);

					TestConsole.WriteLine(JsonSerializer.Serialize(model, options));
				}
				catch (Exception e)
				{
					HandleException(e);
					Assert.That(e.Message, Is.EqualTo("JSON does not meet schema requirements"));
					throw;
				}
			}
		);
	}

	/// <summary>
	/// The validation result is passed in the <see cref="Exception.Data"/>
	/// property under the `"validation"` key.
	/// </summary>
	private static void HandleException(Exception e)
	{
		TestConsole.WriteLine(e);
		TestConsole.WriteLine();
		if (e.Data.Contains("validation"))
		{
			var validation = (EvaluationResults)e.Data["validation"]!;
			TestConsole.WriteLine(JsonSerializer.Serialize(validation, _options));
		}
	}
}