***NOTE** This documentation is based on the the latest non-beta version.  Updated documentation is in progress and will be available soon.*

# Overview

The occasion may arise when you wish to validate that a JSON object is in the correct form (has the appropriate keys and the right types of values).  Enter JSON Schema.  Much like XML Schema with XML, JSON Schema defines a pattern for JSON data.  A JSON Schema validator can verify that a given JSON object meets the requirements as defined by the JSON Schema.  This validation can come in handy as a precursor step before deserializing.

More information about JSON Schema can be found at [json-schema.org](http://json-schema.org).

To support JSON Schema, JsonSchema.Net exposes the `JsonSchema` type.  This type is implemented as a list of keywords, each of which correspond to one of the keywords defined in the JSON Schema specifications.

## Drafts

There are currently six active drafts of the JSON Schema specification:

- Draft 3
- Draft 4
- Draft 6
- Draft 7
- Draft 2019-09
- Draft 2020-12

JsonSchema.Net supports draft 6 and later.

### Meta-schemas

Each draft defines a meta-schema.  This is a special JSON Schema that describes all of the keywords available for that draft.  They are intended to be used to validate other schemas.  Usually, a schema will declare the draft it should adhere to using the `$schema` keyword.

JsonSchema.Net declares the meta-schemas for the supported drafts as members of the `MetaSchemas` static class.

Draft 2019-09 introduced vocabularies.  As part of this new feature, the meta-schemas for this draft and those which follow it have been split into vocabulary-specific meta-schemas.  Additionally, the specification recognizes that the meta-schemas aren't perfect and may need to be updated occasionally.  To this end, the meta-schemas are versioned with the year and month they are published.  The schemas within this library are named accordingly.

## Keywords

JSON Schema is expressed as a collection of keywords, each of which provides a specific constraint on a JSON instance.  For example, the `type` keyword specifies what type of data an instance may be, whereas the `minimum` keyword specifies a minimum numeric value *for numeric data*.  These keywords can be combined to express the expected shape of any JSON instance.  Once defined, the schema validates the instance, providing feedback on errors that occurred, including what and where the error occurred.

# Building a schema

There are two options when building a schema: defining it inline using the fluent builder and defining it externally and deserializing.  Which method you use depends on your specific requirements.

## Deserialization

JsonSchema.Net schemas are fully serializable.

```c#
var mySchema = JsonSchema.FromText(content);
```

which just does

```c#
var mySchema = JsonSerializer.Deserialize<JsonSchema>(content);
```

Done.

## Inline

There are many reasons why you would want to hard-code your schemas.  This library actually hard-codes all of the meta-schemas.  Whatever your reason, the `JsonSchemaBuilder` class is going to be your friend.

The builder class itself is pretty simple.  It just has an `.Add()` method which takes an instance of `IJsonSchemaKeyword`.  The real power comes from the multitudes of extension methods.  There's at least one for every keyword, and they all take the appropriate types for the data that the keyword expects.

Once you've added all of your properties, just call the `.Build()` method to get your schema object.

```c#
var builder = new JsonSchemaBuilder()
    // builder methods
    ;
var schema = builder.Build();
```

Let's take a look at some of the builder extension methods.

### Easy Mode

Some of the more straightforward builder methods are for like the `title` and `$comment` keywords, which just take a string:

```c#
builder.Comment("a comment")
    .Title("A title for my schema");
```

Notice that these methods implement a fluent interface so that you can chain them together.

### A Little Spice

Other extension methods can take multiple values.  These have been overloaded to accept both `IEnumerable<T>` and `params` arrays just to keep things flexible.

```c#
var required = new List<string>{"prop1", "prop2"};
builder.Required(required);
```

or just

```c#
builder.Required("prop1", "prop2");
```

### Now You're Cooking With Gas

Lastly, we have the extension methods which take advantage of C# 7 tuples.  These include keywords like `$defs` and `properties` which take objects in their JSON form.

```c#
builder.Properties(
        ("prop1", new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .MinLength(8)
        ),
        ("prop2", new JsonSchemaBuilder()
            .Type(SchemaValueType.Number)
            .MultipleOf(42)
        )
    );
```

Did you notice how the `JsonSchemaBuilder` is just included directly without the `.Build()` method?  These methods actually require `JsonSchema` objects.  This leads us into the next part.

### Conversions

`JsonSchemaBuilder` defines an implicit cast to `JsonSchema` which calls the `.Build()` method.

To help things further, `JsonSchema` also defines implicit conversions from `bool`.  This allows you to simply use `true` and `false` to create their respective schemas.

```c#
builder.Properties(
        ("prop1", new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .MinLength(8)
        ),
        ("prop2", new JsonSchemaBuilder()
            .Type(SchemaValueType.Number)
            .MultipleOf(42)
        ),
        ("prop3", true)
    );
```

This cast can be used anywhere a `JsonSchema` is needed, such as in the `additionalProperties` or `items` keywords.

```c#
builder.Properties(
        ("prop1", new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .MinLength(8)
        ),
        ("prop2", new JsonSchemaBuilder()
            .Type(SchemaValueType.Number)
            .MultipleOf(42)
        ),
        ("prop3", true),
        ("prop4", new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(true)
        )
    )
    .AdditionalProperties(false);
```

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
        ("myProperty", new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .MinLength(10)
        )
    )
    .Required("myProperty");

// you can build or parse you JsonNode however you like
var emptyJson = new JsonObject();
var booleanJson = JsonNode.Parse("{\"myProperty\":false}");
var stringJson = new JsonObject { ["myProperty"] = "some string" };
var shortJson = new JsonObject { ["myProperty"] = "short" };
var numberJson = new JsonObject { ["otherProperty"] = 35.4 };
var nonObject = JsonNode.Parse("\"not an object\"");

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

The `format` keyword has been around a while.  It's available in all of the drafts supported by JsonSchema.Net.  Although this keyword is technically classified as an annotation, the specification does allow (the word used is "SHOULD") that implementation provide some level of validation on it so long as that validation may be configured on and off.

JsonSchema.Net makes a valiant attempt at validating a few of them.  These are hardcoded as static fields on the `Formats` class.  Out of the box, these are available:

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
- `uri-template` (not supported by default, see [this user solution](https://github.com/gregsdennis/json-everything/issues/244#issuecomment-1081842470) for a workaround)

I'm not going to claim that the validation on any of these is perfect, but it will likely suffice for most applications.  In the (rare) event that it doesn't support your needs, they are completely overridable.  Additionally, if you find a method that works better than what's in the library, feel free to [contribute a PR](https://github.com/gregsdennis/json-everything/pulls) and make the library better for everyone.

New formats must be registered via the `Formats.Register()` static method.  This method will overwrite any existing registration for that format, so this can be used to override the default behavior for known formats.  This also makes the format available for deserialization.

***IMPORTANT** Format implementations MUST not contain state as the same instance will be shared by all of the schema instances that use it.

## Options

The `ValidationOptions` class gives you a few configuration points for customizing how the validation process behaves.  It is an instance class and can be passed into the `JsonSchema.Validate()` method.  If no options are explicitly passed, a copy of `JsonSchemaOptions.Default` will be used.

- `ValidateAs` - Indicates which schema draft to process as.  This will filter the keywords of a schema based on their support.  This means that if any keyword is not supported by this draft, it will be ignored.
- `ValidateMetaSchema` - Indicates whether the schema should be validated against its `$schema` value (its meta-schema).  This is not typically necessary.  Note that the evaluation process will still attempt to resolve the meta-schema. \*
- `OutputFormat` - You already read about output formats above.  This is the property that controls it all.  By default, a single "flag" node is returned.  This also yields the fastest validation times as it enables certain optimizations.
- `ProcessCustomKeywords` - For schema versions which support the vocabulary system (i.g. 2019-09 and after), allows custom keywords to be processed which haven't been included in a vocabulary.  This still requires the keyword type to be registered with `SchemaRegistry`.

_\* If you're using a custom meta-schema, you'll need to load it per the [Schema Registration](json-schema#schema-registration) section below.  Custom meta-schemas form a chain of meta-schemas (e.g. your custom meta-schema may reference another which references the draft 7 meta-schema).  Ultimately, the chain MUST end at a JSON-Schema-defined meta-schema as this defines the processing rules for the schema.  An error will be produced if the meta-schema chain ends at a meta-schema that is unrecognized._

# Managing references (`$ref`)

JsonSchema.Net handles all references as defined in the draft 2020-12 version of the JSON Schema specification.  What this means for draft 2019-09 and later schemas is that `$ref` can now exist alongside other keywords; for earlier drafts, keywords as siblings to `$ref` will be ignored.

## Schema registration

In order to resolve references more quickly, JsonSchema.Net maintains two schema registries for all schemas and subschemas that it has encountered.  The first is a global registry, and the second is a local registry that is passed around on the validation context.  If a schema is not found in the local registry, it will automatically fall back to the global registry.

A `JsonSchema` instance will automatically register itself upon calling `Validate()`.  However, there are some cases where this may be insufficient.  For example, in cases where schemas are separated across multiple files, it is necessary to register the schema instances prior to validation.

For example, given these two schemas

```json
{
  "$id": "http://localhost/my-schema",
  "$type": "object",
  "properties": {
    "refProp": { "$ref": "http://localhost/random-string" }
  }
}

{
  "$id": "http://localhost/random-string",
  "type": "string"
}
```

Here's the schema build inline:

```c#
var schema = new JsonSchemaBuilder()
    .Id("http://localhost/my-schema")
    .Type(SchemaValueType.Object)
    .Properties(("refProp", new JsonSchemaBuilder().Ref("http://localhost/random-string")))
    .Build();
```

You must register `random-string` before you attempt to validate with `my-schema`.

```c#
var randomString = JsonSchema.FromFile("random-string.json");
SchemaRegistry.Global.Register("http://localhost/random-string", randomString);
```

Now JsonSchema.Net will be able to resolve the reference.

## Automatic resolution

In order to support scenarios where schemas cannot be registered ahead of time, the `SchemaRegistry` class exposes the `Fetch` property which is defined as `Func<Uri, JsonSchema>`.  This property can be set to a method which downloads the content from the supplied URI and deserializes it into a `JsonSchema` object.

The URI that is passed may need to be transformed, based on the schemas you're dealing with.  For instance if you're loading schemas from a local filesystem, and the schema `$ref`s use relative paths, you may need to prepend the working folder to the URI in order to locate it.

# Error reporting

Beginning with v2.3.0, the library exposes the `ErrorMessages` static type which includes read/write properties for all of the error messages.  Customization of error messages can be achieved by setting these properties.

## Templates

Most of the error messages support token replacement.  Tokens will use the format `[[foo]]` and will be replaced by the JSON serialization of the associated value.

For example, the default message for `minimum` is:

```
[[received]] is less than or equal to [[limit]]
```

In this case, `[[received]]` will be replaced by the value in the JSON instance, and `[[limit]]` will be replaced by the value of the `minimum` keyword.  This results in a message similar to:

```
5 is less than or equal to 10
```

***NOTE** Since this example uses numbers, they appear without any particular formatting as this is how numbers serialize into JSON.  Similarly, strings will render surrounded by double quotes, `true`, `false`, and `null` will appear using those literals, and more complex values like object and arrays will be rendered in their JSON representation.*

## Localization

In addition to customization, using resource files enables support for localization.  The default locale is determined by `CultureInfo.CurrentCulture` and can be overridden by setting the `ErrorMessages.Culture` static property.

At initial release, the only additional language provided was Spanish (provided by Google Translate 😁).  PRs are welcome to help create additional translations.