## Summary

_JsonSchema.Net.DataGeneration_ extends [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) by adding support to generate JSON data that conforms to a given schema.

## Links

- [Documentation](https://docs.json-everything.net/schema/datagen/schema-datagen/)
- [API Reference](https://docs.json-everything.net/api/JsonSchema.Net.DataGeneration/JsonSchemaExtensions/)
- [Release Notes](https://docs.json-everything.net/rn-json-schema-datageneration/)

## Usage

```c#
var schema = JsonSchema.FromFile("myFile.json");
var generationResult = schema.GenerateData();
var sampleData = generationResult.Result;
```