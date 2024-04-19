## Summary

_JsonPath.Net_ implements the JSON Path specification [RFC 9535](https://www.rfc-editor.org/rfc/rfc9535.html), a string syntax for selecting and extracting JSON values from within a given JSON value.

## Links

- [Documentation](https://docs.json-everything.net/path/basics/)
- [API Reference](https://docs.json-everything.net/api/JsonPath.Net/JsonPath/)
- [Release Notes](https://docs.json-everything.net/rn-json-path/)

## Usage

```c#
var path = JsonPath.Parse("$.prop[0:6:2]");
var instance = JsonNode.Parse("{\"prop\":[0,1,2,3]}");

var results = path.Evaluate(instance);

/*
results:
[
  {
    "Value": 0,
    "Location": "$['prop'][0]"
  },
  {
    "Value": 2,
    "Location": "$['prop'][2]"
  }
]
*/
```
