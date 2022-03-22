# JSON Schema Vocabularies

JSON Schema draft 2019-09 introduced the idea of vocabularies to enable some spec support for custom keywords.

`json-everything` defines two such vocabularies:

- [JsonSchema.Net.Data](./vocabs-data.md) allows a schema to reference instance or external data to be used in existing keywords.  This has been a highly requested feature since the early days of JSON Schema.
- [JsonSchema.Net.UniqueKeys](./vocabs-unique-keys.md) defines a new keyword that allows the schema author to define that items in an array must have unique child values, or even unique combinations of child values to be considered distinct.
