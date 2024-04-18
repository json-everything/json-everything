## Summary

_JsonSchema.Net.Generation_ extends [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) by adding support to generate schemas from .Net types.

## Links

- [Documentation](https://docs.json-everything.net/schema/schemagen/schema-generation/)
- [API Reference](https://docs.json-everything.net/api/JsonSchema.Net.Generation/JsonSchemaBuilderExtensions/)
- [Release Notes](https://docs.json-everything.net/rn-json-schema-generation/)

## Usage

```c#
var schema = new JsonSchemaBuilder()
    .FromType<MyType>()
    .Build();
```