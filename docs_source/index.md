# Welcome!

`json-everything` is your one-stop shop for all things JSON, powered by .Net's very own JSON models and serializer in the `System.Text.Json` namespace.

The libraries contained here are built for speed and designed for usability.

## The Libraries

Currently there are three libraries output by this project:

### Json.More.Net

Json.More.Net provides some core functionality that one would expect to be in `System.Text.Json` but isn't.  These include simple value conversion and equality comparisons involving their object model.

### JsonPointer.Net
JsonPointer.Net provides an implementation of JSON Pointer ([RFC 6901](https://tools.ietf.org/html/rfc6901)) and a variant called [Relative JSON Pointer](), which is an important component in JSON HyperSchema.

***NOTE** Relative JSON Pointer requires the ability to navigate up toward the root of the JSON structure, which is [not supported yet](https://github.com/dotnet/runtime/issues/40452).  While this library can model and validate relative pointers, it cannot evaluate them until this functionality is available.*

### JsonSchema.Net

JsonSchema.Net provides [JSON Schema](https://json-schema.org) support for drafts 6 and after.  It is 100% compliant and passes all required and most optional tests of the [JSON Schema Test Suite](https://github.com/json-schema-org/JSON-Schema-Test-Suite).

***NOTE** The remaining optional tests involve ECMA Regular Expressions, big-num support, and some string formats.*

### JsonPath.Net

JsonPath.Net provides JSON querying using the JSON Path querying language inspired by XPath and [conceptualized by Matt Goessner](https://goessner.net/articles/JsonPath/).  A standard is currently in progress for JSON Path.  This library will update as the spec evolves.