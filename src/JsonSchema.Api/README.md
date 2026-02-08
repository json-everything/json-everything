## Summary

_JsonSchema.Net.Api_ provides automatic request body validation powered by [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) and [_JsonSchema.Net.Generation_](https://www.nuget.org/packages/JsonSchema.Net.Generation).

## Links

- [Documentation](https://docs.json-everything.net/schema/schemagen/schema-api/)
- [API Reference](https://docs.json-everything.net/api/JsonSchema.Net.Api/)
- [Release Notes](https://docs.json-everything.net/rn-json-schema-api/)

## Usage

```c#
var schema = new JsonSchemaBuilder()
    .FromType<MyType>()
    .Build();
```

## Sponsorship

If you found this library helpful and would like to promote continued development, please consider [sponsoring the maintainers](https://github.com/sponsors/gregsdennis).