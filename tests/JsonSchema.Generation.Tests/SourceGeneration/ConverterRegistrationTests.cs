using System.Linq;
using System.Text.Json;
using Json.Schema.Generation.Serialization;
using NUnit.Framework;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Json.Schema.Generation.Tests.SourceGeneration;

public class ConverterRegistrationTests
{
	[Test]
	public void GeneratedCode_ContainsRegistrationClass()
	{
		var assembly = typeof(TestModels).Assembly;
		var registrationType = assembly.GetTypes()
			.FirstOrDefault(t => t.Name == "GeneratedSchemaRegistration");
		
		Assert.That(registrationType, Is.Not.Null, "GeneratedSchemaRegistration class should be generated");
	}

	[Test]
	public void GeneratedCode_ContainsModuleInitializer()
	{
		var assembly = typeof(TestModels).Assembly;
		var registrationType = assembly.GetTypes()
			.FirstOrDefault(t => t.Name == "GeneratedSchemaRegistration");
		
		Assert.That(registrationType, Is.Not.Null);
		
		var registerMethod = registrationType?.GetMethod("RegisterSchemas", 
			System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		
		Assert.That(registerMethod, Is.Not.Null, "RegisterSchemas method should exist");
		
		var moduleInitAttr = registerMethod?.GetCustomAttributes(false)
			.FirstOrDefault(a => a.GetType().Name == "ModuleInitializerAttribute");
		
		Assert.That(moduleInitAttr, Is.Not.Null, "RegisterSchemas should have ModuleInitializer attribute");
	}

	[Test]
	public void ConvertersAreRegistered_AfterModuleInitialization()
	{
		var options = new JsonSerializerOptions{ TypeInfoResolverChain = { TestSerializerContext.Default } };
		options.Converters.Add(new GenerativeValidatingJsonConverter());
		
		var person = new TestModels.SimplePerson { Name = "Alice", Age = 30 };
		var json = JsonSerializer.Serialize(person, options);
		
		Assert.That(json, Does.Contain("Alice"));
		
		var deserialized = JsonSerializer.Deserialize<TestModels.SimplePerson>(json, options);
		
		Assert.That(deserialized, Is.Not.Null);
		Assert.That(deserialized!.Name, Is.EqualTo("Alice"));
		Assert.That(deserialized.Age, Is.EqualTo(30));
	}

	[Test]
	public void InvalidJson_ThrowsWithValidation()
	{
		var options = new JsonSerializerOptions { TypeInfoResolverChain = { TestSerializerContext.Default } };
		options.Converters.Add(new GenerativeValidatingJsonConverter());

		var invalidJson = """{"Name": "Bob", "Age": "not-a-number"}""";
		
		Assert.Throws<JsonException>(() =>
		{
			JsonSerializer.Deserialize<TestModels.SimplePerson>(invalidJson, options);
		});
	}

	[Test]
	public void MultipleTypes_AllRegistered()
	{
		var options = new JsonSerializerOptions
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			PropertyNameCaseInsensitive = true
		};
		options.Converters.Add(new GenerativeValidatingJsonConverter());

		// Test SimplePerson (PascalCase properties)
		var json1 = """{"Name": "Alice", "Age": 30}""";
		var deserialized1 = JsonSerializer.Deserialize<TestModels.SimplePerson>(json1, options);
		Assert.That(deserialized1, Is.Not.Null);
		Assert.That(deserialized1!.Name, Is.EqualTo("Alice"));
		Assert.That(deserialized1.Age, Is.EqualTo(30));
		
		// Test CamelCasePerson (camelCase properties as per schema attribute)
		var json2 = """{"firstName": "Bob", "lastName": "Smith", "age": 25}""";
		var deserialized2 = JsonSerializer.Deserialize<TestModels.CamelCasePerson>(json2, options);
		Assert.That(deserialized2, Is.Not.Null);
		Assert.That(deserialized2!.FirstName, Is.EqualTo("Bob"));
		Assert.That(deserialized2.LastName, Is.EqualTo("Smith"));
		Assert.That(deserialized2.Age, Is.EqualTo(25));
	}
}
