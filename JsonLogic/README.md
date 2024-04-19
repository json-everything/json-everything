## Summary

_JsonLogic_ implements the [JSON Logic](https://jsonlogic.com/) format, a declarative, data-driven way to define rules.

## Links

- [Documentation](https://docs.json-everything.net/logic/basics/)
- [API Reference](https://docs.json-everything.net/api/JsonLogic/JsonLogic/)
- [Release Notes](https://docs.json-everything.net/rn-json-logic/)

## Usage

```c#
var rule = JsonNode.Parse(
    """
    {
      "and": [
        { "<": [{ "var": "temp" }, 110 ]},
        { "==": [{ "var": "pie.filling" }, "apple" ]}
      ]
    }
    """
);
var data = JsonNode.Parse(
    """
    {
      "temp" : 100,
      "pie" : { "filling" : "apple" }
    }
    """
);
var result = JsonLogic.Apply(rule, data);

// result: true
```
