# Validation & annotations

## Validating instances

`JsonSchema` exposes a `Validate()` method which is used to validate JSON instances.  Let's begin with the following schema and a few JSON objects:

```json 
{
  "properties":{
    "myProperty":{
      "type":"string",
      "minLength":10
    }
  },
  "required":["myProperty"]
}

{}
{"myProperty":false}
{"myProperty":"some string"}
{"otherProperty":35.4}
"nonObject"
```

To validate these, all we have to do is pass these into our schema's `Validate(JsonElement)` method.

```csharp
JsonSchema schema = new JsonSchemaBuilder()
    .Properties(
        (
            "myProperty", new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .MinLength(10)
        )
    )
    .Required("myProperty");
var emptyJson = JsonDocument.Parse("{}").RootElement;
var booleanJson = JsonDocument.Parse("{\"myProperty\":false}").RootElement;
var stringJson = JsonDocument.Parse("{\"myProperty\":\"some string\"}").RootElement;
var shortJson = JsonDocument.Parse("{\"myProperty\":\"short\"}").RootElement;
var numberJson = JsonDocument.Parse("{\"otherProperty\":35.4}").RootElement;
var nonObject = JsonDocument.Parse("\"not an object\"").RootElement;

var emptyResults = schema.Validate(emptyJson);
var booleanResults = schema.Validate(booleanJson);
var stringResults = schema.Validate(stringJson);
var shortResults = schema.Validate(shortJson);
var numberResults = schema.Validate(numberJson);
var nonObjectResults = schema.Validate(nonObject);
```

The various results objects are of type `ValidationResults`.  More information about the results object can be found in the next section.

In the above example, the following would result:

- `emptyJson` and `numberJson` failed because `"myProperty"` was not found.
- `booleanJson` failed because the value of `"myProperty"` is of the wrong type.
- `stringJson` passed validation.
- `shortJson` failed because the value of `"myProperty"` was too short.
- `nonObject` also passed validation because `properties` and `required` ignore non-object JSON.

No errors would actually be reported here because the output format defaults to a "flag" format, which is a basic pass/fail.  To get specific errors, the output format will need to be configured.

## Validation results

JSON Schema draft 2019-09 standardized the format for validation output in order to support cross-platform and cross-implementation compatibility.  The format is described in the [JSON Schema spec, section 10](https://json-schema.org/draft/2019-09/json-schema-core.html#rfc.section.10).  This includes support for both errors and annotation collection.

In summary, there are four levels of verbosity for output: Flag, Basic, Detailed, and Verbose.

The flag format will simply return a boolean value indicating a pass/fail result.  All other formats include JSON Pointers and URIs to indicate the source of the errors or annotations that were produced.

A basic format reduces all of the errors to a flat list.

The detailed and verbose output formats follows the hierarchical structure of the schema.  The verbose one copies this structure exactly, whereas the standard hierarchy will condense the results where possible.

The default output format is Flag, but this can be configured via the `ValidationOptions.OutputFormat` static property.

***NOTE** It's only possible to translate from a more detailed to a less detailed format.*

### Examples of output

<details>
  <summary>Verbose Hierarchy</summary>

```json
{
  "valid" : false,
  "keywordLocation" : "#",
  "instanceLocation" : "#",
  "errors" : [
      {
        "valid" : false,
        "keywordLocation" : "#/allOf",
        "instanceLocation" : "#",
        "keyword" : "allOf",
        "errors" : [
            {
              "valid" : false,
              "keywordLocation" : "#/allOf/0",
              "instanceLocation" : "#",
              "errors" : [
                  {
                    "valid" : false,
                    "keywordLocation" : "#/allOf/0/type",
                    "instanceLocation" : "#",
                    "keyword" : "type",
                    "additionalInfo" : {
                        "expected" : "array",
                        "actual" : "object"
                      }
                  }
                ]
            },
            {
              "valid" : false,
              "keywordLocation" : "#/allOf/1",
              "instanceLocation" : "#",
              "errors" : [
                  {
                    "valid" : false,
                    "keywordLocation" : "#/allOf/1/type",
                    "instanceLocation" : "#",
                    "keyword" : "type",
                    "additionalInfo" : {
                        "expected" : "number",
                        "actual" : "object"
                      }
                  }
                ]
            }
          ]
      }
    ]
}
```

</details><br>

<details>
  <summary>Condensed Hierarchy</summary>

```json
{
  "valid" : false,
  "keywordLocation" : "#/allOf",
  "instanceLocation" : "#",
  "keyword" : "allOf",
  "errors" : [
      {
        "valid" : false,
        "keywordLocation" : "#/allOf/0/type",
        "instanceLocation" : "#",
        "keyword" : "type",
        "additionalInfo" : {
            "expected" : "array",
            "actual" : "object"
          }
      },
      {
        "valid" : false,
        "keywordLocation" : "#/allOf/1/type",
        "instanceLocation" : "#",
        "keyword" : "type",
        "additionalInfo" : {
            "expected" : "number",
            "actual" : "object"
          }
      }
    ]
}
```

</details><br>

<details>
  <summary>Flat List</summary>

```json
{
  "valid" : false,
  "errors" : [
      {
        "valid" : false,
        "keywordLocation" : "#/allOf",
        "instanceLocation" : "#",
        "keyword" : "allOf"
      },
      {
        "valid" : false,
        "keywordLocation" : "#/allOf/0/type",
        "instanceLocation" : "#",
        "keyword" : "type",
        "additionalInfo" : {
            "expected" : "array",
            "actual" : "object"
          }
      },
      {
        "valid" : false,
        "keywordLocation" : "#/allOf/1/type",
        "instanceLocation" : "#",
        "keyword" : "type",
        "additionalInfo" : {
            "expected" : "number",
            "actual" : "object"
          }
      }
    ]
}
```

</details>

## Value format validation

The `format` keyword has been around a while.  It's available in all of the drafts supported by JsonSchema<nsp>.Net.  Although this keyword is techincally classified as an annotation, the specification does allow (the word used is "SHOULD") that implementation provide some level of validation on it so long as that validation may be configured on and off.

JsonSchema<nsp>.Net makes a valiant attempt at validating a few of them.  These are hardcoded as static fields on the `Formats` class.  Out of the box, these are available:

- `date`
- `date-time`
- `duration`
- `email`
- `hostname`
- `ipv4`
- `ipv6`
- `iri`
- `iri-reference`
- `json-pointer`
- `regex`
- `relative-json-pointer`
- `uri`
- `uri-reference`
- `uri-template`

I'm not going to claim that the validation on any of these is perfect, but it will likely suffice for most applications.  In the (rare) event that it doesn't support your needs, they are completely overridable.  Additionally, if you find a method that works better than what's in the library, feel free to [contribute a PR](https://github.com/gregsdennis/json-everything/pulls) and make the library better for everyone.

New formats can be registered via the `Formats.Register()` static method.  This method will overwrite any existing registration for that format, so this can be used to override the default behavior for known formats.  This also makes the format available for deserialization.

***IMPORTANT** Format implementations MUST not contain state as the same instance will be shared by all of the schema instances that use it.

## Options

The `ValidationOptions` class gives you a few configuration points for customizing how the validation process behaves.  It is an instance class and can be passed into the `JsonSchema.Validate()` method.  If no options are explicitly passed, a copy of `JsonSchemaOptions.Default` will be used.

- `ValidateAs` - Indicates which schema draft to process as.  This will filter the keywords of a schema based on their support.  This means that if any keyword is not supported by this draft, it will be ignored.
- `ValidateMetaSchema` - Indicates whether the schema should be validated against its `$schema` value (its meta-schema).  This is not typically necessary.
- `OutputFormat` - You already read about output formats above.  This is the property that controls it all.  By default, a single "flag" node is returned.  This also yields the fastest validation times it enables certain optimizations.
