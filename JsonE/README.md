## Summary

_JsonE.Net_ implements the [JSON-e](https://json-e.js.org/) format, a data-structure parameterization system for embedding context in JSON objects.

## Links

- [Documentation](https://docs.json-everything.net/json-e/basics/)
- [API Reference](https://docs.json-everything.net/api/JsonE.Net/JsonE/)
- [Release Notes](https://docs.json-everything.net/rn-json-e/)

## Usage

```c#
var template = JsonNode.Parse(
    "{\"$eval\": \"(z / x) ** 2\"}"
);
var context = JsonNode.Parse(
    "{\"x\": 10, \"z\": 20, \"s\": \"face\", \"t\": \"plant\"}"
)!.AsObject();

var result = JsonE.Evaluate(template, context);

// result: 4
```
