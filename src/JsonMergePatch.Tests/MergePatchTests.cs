using System.Text.Json;
using DotNext.Text.Json;
using NUnit.Framework;

namespace Json.MergePatch.Tests;

[TestFixture]
public class MergePatchTests
{
	private static readonly JsonSerializerOptions _options = new()
	{
		PropertyNameCaseInsensitive = true
	};

	static MergePatchTests()
	{
		_options.Converters.Add(new OptionalConverterFactory());
	}

	[Test]
	public void ApplyTo_UpdatesPrimitiveProperty()
	{
		var forecast = new WeatherForecast { Temperature = 20, Summary = "Sunny" };

		var patch = """{ "temperature": 30 }""";
		var dto = JsonSerializer.Deserialize<WeatherForecast.UpdateDto>(patch, _options)!;
		dto.ApplyTo(forecast);

		Assert.That(forecast.Temperature, Is.EqualTo(30));
		Assert.That(forecast.Summary, Is.EqualTo("Sunny"));
	}

	[Test]
	public void ApplyTo_NullValueClearsNullableProperty()
	{
		var forecast = new WeatherForecast { Temperature = 20, Summary = "Sunny" };

		var patch = """{ "summary": null }""";
		var dto = JsonSerializer.Deserialize<WeatherForecast.UpdateDto>(patch, _options)!;
		dto.ApplyTo(forecast);

		Assert.That(forecast.Summary, Is.Null);
		Assert.That(forecast.Temperature, Is.EqualTo(20));
	}

	[Test]
	public void ApplyTo_MissingPropertyLeavesValueUnchanged()
	{
		var forecast = new WeatherForecast { Temperature = 20, Summary = "Sunny" };

		var patch = """{ "temperature": 25 }""";
		var dto = JsonSerializer.Deserialize<WeatherForecast.UpdateDto>(patch, _options)!;
		dto.ApplyTo(forecast);

		Assert.That(forecast.Summary, Is.EqualTo("Sunny"));
	}

	[Test]
	public void ApplyTo_NullNestedObjectSetsPropertyToNull()
	{
		var forecast = new WeatherForecast
		{
			Temperature = 20,
			Location = new Location { City = "London" }
		};

		var patch = """{ "location": null }""";
		var dto = JsonSerializer.Deserialize<WeatherForecast.UpdateDto>(patch, _options)!;
		dto.ApplyTo(forecast);

		Assert.That(forecast.Location, Is.Null);
	}

	[Test]
	public void ApplyTo_RecursivelyPatchesNestedObject()
	{
		var forecast = new WeatherForecast
		{
			Temperature = 20,
			Location = new Location { City = "London", PostalCode = "SW1A" }
		};

		var patch = """{ "location": { "city": "Paris" } }""";
		var dto = JsonSerializer.Deserialize<WeatherForecast.UpdateDto>(patch, _options)!;
		dto.ApplyTo(forecast);

		Assert.That(forecast.Location!.City, Is.EqualTo("Paris"));
		Assert.That(forecast.Location.PostalCode, Is.EqualTo("SW1A"));
	}

	[Test]
	public void ApplyTo_CreatesNestedObjectWhenNull()
	{
		var forecast = new WeatherForecast { Temperature = 20, Location = null };

		var patch = """{ "location": { "city": "Rome" } }""";
		var dto = JsonSerializer.Deserialize<WeatherForecast.UpdateDto>(patch, _options)!;
		dto.ApplyTo(forecast);

		Assert.That(forecast.Location, Is.Not.Null);
		Assert.That(forecast.Location!.City, Is.EqualTo("Rome"));
	}

	[Test]
	public void ApplyTo_RespectsJsonPropertyNameAttribute()
	{
		var location = new Location { City = "Berlin", PostalCode = "10115" };

		var patch = """{ "zip": "12345" }""";
		var dto = JsonSerializer.Deserialize<Location.UpdateDto>(patch, _options)!;
		dto.ApplyTo(location);

		Assert.That(location.PostalCode, Is.EqualTo("12345"));
		Assert.That(location.City, Is.EqualTo("Berlin"));
	}

	[Test]
	public void ApplyTo_UnknownPropertiesAreIgnored()
	{
		var forecast = new WeatherForecast { Temperature = 20 };

		var patch = """{ "unknownField": "value", "temperature": 99 }""";
		var dto = JsonSerializer.Deserialize<WeatherForecast.UpdateDto>(patch, _options)!;
		dto.ApplyTo(forecast);

		Assert.That(forecast.Temperature, Is.EqualTo(99));
	}

	[Test]
	public void ApplyTo_ReplacesArrayCollection()
	{
		var model = new CollectionContainer
		{
			IntArray = [1, 2, 3]
		};

		var patch = """{ "intArray": [7, 8] }""";
		var dto = JsonSerializer.Deserialize<CollectionContainer.UpdateDto>(patch, _options)!;
		dto.ApplyTo(model);

		Assert.That(model.IntArray, Is.EqualTo([7, 8]));
	}

	[Test]
	public void ApplyTo_ReplacesListCollection()
	{
		var model = new CollectionContainer
		{
			IntList = [1, 2, 3]
		};

		var patch = """{ "intList": [10, 20] }""";
		var dto = JsonSerializer.Deserialize<CollectionContainer.UpdateDto>(patch, _options)!;
		dto.ApplyTo(model);

		Assert.That(model.IntList, Is.EqualTo([10, 20]));
	}

	[Test]
	public void ApplyTo_NullCollectionClearsProperty()
	{
		var model = new CollectionContainer
		{
			Tags = ["red", "blue"]
		};

		var patch = """{ "tags": null }""";
		var dto = JsonSerializer.Deserialize<CollectionContainer.UpdateDto>(patch, _options)!;
		dto.ApplyTo(model);

		Assert.That(model.Tags, Is.Null);
	}

	[Test]
	public void ApplyTo_MissingCollectionLeavesValueUnchanged()
	{
		var model = new CollectionContainer
		{
			IntList = [3, 4, 5]
		};

		var patch = "{}";
		var dto = JsonSerializer.Deserialize<CollectionContainer.UpdateDto>(patch, _options)!;
		dto.ApplyTo(model);

		Assert.That(model.IntList, Is.EqualTo([3, 4, 5]));
	}

	[Test]
	public void ApplyTo_ReplacesComplexObjectCollection()
	{
		var model = new CollectionContainer
		{
			Locations =
			[
				new Location { City = "Berlin", PostalCode = "10115" },
				new Location { City = "London", PostalCode = "SW1A" }
			]
		};

		var patch =
			"""
			{
			  "locations": [
			    { "city": "Paris", "zip": "75000" }
			  ]
			}
			""";
		var dto = JsonSerializer.Deserialize<CollectionContainer.UpdateDto>(patch, _options)!;
		dto.ApplyTo(model);

		Assert.That(model.Locations, Has.Count.EqualTo(1));
		Assert.That(model.Locations![0].City, Is.EqualTo("Paris"));
		Assert.That(model.Locations[0].PostalCode, Is.EqualTo("75000"));
	}
}
