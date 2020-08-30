# JSON Schema Overview

The occasion may arise when you wish to validate that a JSON object is in the correct form (has the appropriate keys and the right types of values).  Enter JSON Schema.  Much like XML Schema with XML, JSON Schema defines a pattern for JSON data.  A JSON Schema validator can verify that a given JSON object meets the requirements as defined by the JSON Schema.  This validation can come in handy as a precursor step before deserializing.

More information about JSON Schema can be found at [json-schema.org](http://json-schema.org).

To support JSON Schema, Manatee.Json exposes the `JsonSchema` type.  This type is implemented as a list of keywords, each of which correspond to one of the keywords defined in the JSON Schema specifications.

## Drafts

There are currently five active drafts of the JSON Schema specification:

- Draft 03
- Draft 04
- Draft 06
- Draft 07
- Draft 2019-09

Manatee.Json supports draft 04 and later.

### Meta-schemas

Each draft defines a meta-schema.  This is a special JSON Schema that describes all of the keywords available for that draft.  They are intended to be used to validate other schemas.  Usually, a schema will declare the draft it should adhere to using the `$schema` keyword.

Manatee.Json declares the meta-schemas for the supported drafts as members of the `MetaSchemas` static class.

Draft 2019-09 introduced vocabularies.  As part of this new feature, the meta-schema for draft 2019-09 has been split into vocabulary-specific meta-schemas.  Additionally, the specification recognizes that the meta-schemas aren't perfect and may need to be updated occasionally.  To this end, the meta-schemas are versioned with the year and month they are published.  The schemas within this library are named accordingly.

## Keywords

JSON Schema is expressed as a collection of keywords, each of which provides a specific constraint on a JSON instance.  For example, the `type` keyword specifies what type of data an instance may be, whereas the `minimum` keyword specifies a minimum numeric value *for numeric data*.  These keywords can be combined to express the expected shape of any JSON instance.  Once defined, the schema validates the instance, providing feedback on errors that occurred, including what and where the error occurred.
