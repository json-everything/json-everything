# Welcome!

`json-everything` is your one-stop shop for all things JSON, powered by .Net's very own JSON models and serializer in the `System.Text.Json` namespace.

The libraries contained here are built for speed and designed for usability.

## The Libraries

Currently there are seven libraries output by this project:

### Json.More<nsp>.Net

Json.More<nsp>.Net provides some core functionality that one would expect to be in `System.Text.Json` but isn't.  These include simple value conversion and equality comparisons involving their object model.

### JsonPointer<nsp>.Net

JsonPointer.Net provides an implementation of JSON Pointer ([RFC 6901](https://tools.ietf.org/html/rfc6901)) and a variant called [Relative JSON Pointer](), which is an important component in JSON HyperSchema.

***NOTE** Relative JSON Pointer requires the ability to navigate up toward the root of the JSON structure, which is [not supported yet](https://github.com/dotnet/runtime/issues/40452).  While this library can model and validate relative pointers, it cannot evaluate them until this functionality is available.*

### JsonSchema<nsp>.Net

JsonSchema<nsp>.Net provides [JSON Schema](https://json-schema.org) support for drafts 6, 7, 2019-09, and 2020-12.  It is 100% compliant and passes all required and most optional tests of the [JSON Schema Test Suite](https://github.com/json-schema-org/JSON-Schema-Test-Suite).

***NOTE** The remaining optional tests involve ECMA Regular Expressions, big-num support, and some string formats.*

### JsonSchema.Net.Generation

Builds on top of JsonSchema<nsp>.Net to provide schema generation from .Net types.

### JsonSchema.Net.DataGeneration

Builds on top of JsonSchema<nsp>.Net to provide instance (random sample data) generation.

### JsonSchema.Net.Data

Provides an implementation of a [vocabulary](usage/vocabs-data.md) that defines a way to access instance and external data, a long-sought feature of JSON Schema.

### JsonSchema.Net.UniqueKeys

Provides an implementation of a [vocabulary](usage/vocabs-unique-keys.md) that defines a new keyword to validate item uniqueness by idenitifying specific data points within the item.

### JsonPath<nsp>.Net

JsonPath<nsp>.Net provides JSON querying using the JSON Path querying language inspired by XPath and [conceptualized by Matt Goessner](https://goessner.net/articles/JsonPath/).  An [RFC](https://github.com/jsonpath-standard/internet-draft) is currently being developed for JSON Path.  This library will update as the upcoming spec evolves.

### JsonPatch<nsp>.Net

JsonPatch<nsp>.Net provides [JSON Patch](https://tools.ietf.org/html/rfc6902) support which can be used to modify JSON documents.

### JsonLogic

JsonLogic is a .Net implementation of the [JsonLogic concept](https://jsonlogic.com) by [Jeremy Wadhams](https://github.com/jwadhams).