## Summary

_JsonSchema.Net_ fully implements the [JSON Schema](https://json-schema.org/) specifications, a declarative syntax for validation and annotating JSON data.

Supported specifications:

- Draft 6 - `http://json-schema.org/draft-06/schema#`
- Draft 7 - `http://json-schema.org/draft-07/schema#`
- Draft 2019-09 - `https://json-schema.org/draft/2019-09/schema`
- Draft 2020-12 - `https://json-schema.org/draft/2020-12/schema`

This project also operates as a test bed for features proposed for the next version ("draft/next").

## Links

- [Documentation](https://docs.json-everything.net/schema/basics/)
- [API Reference](https://docs.json-everything.net/api/JsonSchema.Net/JsonSchema/)
- [Release Notes](https://docs.json-everything.net/rn-json-schema/)

## Usage

Parse a schema:

```c#
var schema = JsonSchema.FromText(content);
```

Load one from a file:

```c#
var schema = JsonSchema.FromFile(filename);
```

Directly deserialize it:

```c#
var schema = JsonSerializer.Deserialize<JsonSchema>(content);
```

Or build it explicitly in code:

```c#
var element = JsonDocument.Parse(content).RootElement;
var schema = JsonSchema.Build(element);

// or

var fluentSchema = new JsonSchemaBuilder()
    .Comment("a comment")
    .Title("A title for my schema")
    .Type(SchemaValueType.Object)
    .Properties(
        ("foo", new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
        ),
        ("bar", new JsonSchemaBuilder()
            .Type(SchemaValueType.Number)
        )
    )
    .Build();
```

Use the schema to evaluate and annotate data:

```c#
var instance = JsonNode.Parse("{\"foo\":\"a value\",\"bar\":42}");
var results = schema.Evaluate(instance);
```

## Sponsorship

If you found this library helpful and would like to promote continued development, please consider [sponsoring the maintainers](https://github.com/sponsors/gregsdennis).