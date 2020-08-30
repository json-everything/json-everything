# Validation & annotations

## Validating the schema itself

Because the `JsonSchema` class will accept any keyword, and some keywords are only supported by specific drafts, it may be important to ensure that the schema that's created is valid against one of the drafts.

To ensure that this is the case, call the `ValidateSchema()` method.  This will analyze all of the keywords and report on whether they are all compatible.

The results object is an instance of `MetaSchemaValidationResults` which exposes the following properties:

- `IsValid` simply indicates a pass/fail
- `SupportedVersions` indicates the JSON Schema draft versions that this schema passes.
- `MetaSchemaValidations` is a dictionary, keyed by schema ID strings, that contains validation results for those schemas.  Typically the keys will be the IDs of the draft meta-schemas, but could be the ID of any schema.
- `OtherErrors` is a list of strings to support other errors that may result outside of those produced by meta-schema validations.

## Validating instances

`JsonSchema` exposes a `Validate(JsonValue)` method which is used to validate incoming JSON values.  Let's begin with the schema from above and a few JSON objects:

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

To validate these, all we have to do is pass these into our schema's `Validate(JsonValue)` method.

```csharp
var schema = new JsonSchema()
                   .Property("myProperty", new JsonSchema()
                                                 .Type(JsonSchemaType.String)
                                                 .MinLength(10))
                   .Required("myProperty");
var emptyJson = new JsonObject();
var booleanJson = new JsonObject { ["myProperty"] = false };
var stringJson = new JsonObject { ["myProperty"] = "some string" };
var shortJson = new JsonObject { ["myProperty"] = "short" };
var numberJson = new JsonObject { ["otherProperty"] = 35.4 };
var nonObject = (JsonValue)"nonObject";

var emptyResults = schema.Validate(emptyJson);
var booleanResults = schema.Validate(booleanJson);
var stringResults = schema.Validate(stringJson);
var numberResults = schema.Validate(numberJson);
var nonObjectResults = schame.Validate(nonObject);
```

The various results objects are of type `SchemaValidationResults`.  More information about the results object can be found in the next section.

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

The default output format is Flag, but this can be configured via the `JsonSchemaOptions.OutputFormat` static property.

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

The `format` keyword has been around a while.  It's available in all of the drafts supported by Manatee.Json.  Although this keyword is techincally classified as an annotation, the specification does allow (the word used is "SHOULD") that implementation provide some level of validation on it so long as that validation may be configured on and off.

Manatee.Json makes a valiant attempt at validating a few of them.  These are hardcoded as static properties on the `Format` class.  Out of the box, these are available:

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

I'm not going to claim that the validation on any of these is perfect, but it will likely suffice for most applications.  In the (rare) event that it doesn't support your needs, they are completely overridable.

All of the static properties can be set to new instances.  When creating a new instance, it it automatically registered internally (for deserialization purposes) and any lookup by string will result in the newest instance for that key.

In the same way, entirely new formats can be created to make them available to Manatee.Json.

## Options

The `JsonSchemaOptions` class gives you a few configuration points for customizing how the validation process behaves.  It is an instance class and can be passed into the `JsonSchema.Validate()` method.  If no options are explicitly passed, a copy of `JsonSchemaOptions.Default` will be used.

- `Download` - This static function property is the mechanism by which `JsonSchemaRepository` downloads unregistered schemas.  By default, it knows to use `HttpClient` for *http:* endpoints and `System.IO.File` for file paths.  If you need more functionality (for instance if your schema is buried inside an FTP share), override this with a new function that can read from your endpoint.
- `ValidateFormatKeyword` - This defines whether a schema will attempt to apply string format validation based on the value of a `format` keyword.  This is enabled by default.  See above for more information on string format validation.
- `AllowUnknownFormats` - This specifies whether the system will allow unknown string formats.  It is enabled by default.  If `ValidateFormatKeyword` is disabled, this option has no effect.  There are two effect of disabling this option,
  - Validations by schemas with unknown string formats will always return invalid.  This impacts schemas explicitly built in code.
  - If a schema with an unknown string format is deserialized (loaded from an external source), a `JsonSerializationException` will be thrown.
- `OutputFormat` - You already read about output formats above.  This is the property that controls it all.  By default, a collapsed hierarchy is returned.
- `RefResolution` - This defines how the `$ref` keyword is processed.  Prior to draft 2019-09, `$ref`-sibling keywords are to be ignored.  This property can be used to prescribe behavior when the draft cannot be automatically determined.
- `DefaultBaseUri` - When a schema is loaded without an `$id`, it can't know how to properly handle `$ref`s without a base URI.  This property provides that.  The default is `manatee://json-schema/`.  It's can't be externally resolved, but gives something for the system to use.
- `LogMetaSchemaValidation` - This works along with `JsonOptions.Log` and `JsonOptions.LogCategory`.  When a schema is first loaded or used to validate, the system will attempt to validate the schema against its meta-schema (defined by `$schema`).  Logs that are created during this meta-schema validation can be noisy, so this property can be used to disable logs specifically during meta-schema validation.  This will leave the remaining schema validation logging intact.
- `DefaultProcessingVersion` - This property defines a hierarchy of preferred versions to be used for `JsonSchema.ProcessingVersion` when the schema is compatible with multiple drafts.  The default is latest to earliest.

Along with these properties, there are some overrides that allow exclusion of some errors in the output.

- `IgnoreErrorsForChildren<T>()` - This will disable reporting of errors for children of a specific keyword.  This can be useful if you want to ignore errors for e.g. `allOf`.
- `IgnoreErrorsForChildren(JsonPointer)` - This will disable reporting of error for children of a specific path *within the schema*, e.g. a specific `allOf`.
- `ShouldReportChildErrors(IJsonSchemaKeyword, SchemaValidationContext)` - This method can be used by custom keywords to determine whether output should be generated for a given keyword during the validation process.  The return is based on the two above configurations.