## Summary

_JsonSchema.Net.Data_ extends [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) by adding support for the [OpenAPI 3.1 JSON Schema vocabulary](https://spec.openapis.org/oas/latest.html):

- [new formats](https://spec.openapis.org/oas/latest.html#data-types)
- [new keywords](https://spec.openapis.org/oas/latest.html#fixed-fields-19)

## Links

- [Documentation](https://docs.json-everything.net/schema/vocabs/)
- [API Reference](https://docs.json-everything.net/api/JsonSchema.Net.OpenApi/Vocabularies/)
- [Release Notes](https://docs.json-everything.net/rn-json-schema-openapi/)

## Usage

Register the vocabulary:

```c#
using Json.Schema.OpenApi;

Vocabularies.Register();
```

## Sponsorship

If you found this library helpful and would like to promote continued development, please consider [sponsoring the maintainers](https://github.com/sponsors/gregsdennis).