# Overview

The occasion may arise when you wish to validate that a JSON object is in the correct form (has the appropriate keys and the right types of values), or perhaps you wish to annotate that data.  Enter JSON Schema.  Much like XML Schema with XML, JSON Schema defines a pattern for JSON data.  A JSON Schema validator can verify that a given JSON object meets the requirements as defined by the JSON Schema as well as provide additional information to the application about the data.  This evaluation can come in handy as a precursor step before deserializing.

More information about JSON Schema can be found at [json-schema.org](http://json-schema.org).

To support JSON Schema, JsonSchema.Net exposes the `JsonSchema` type.  This type is implemented as a list of keywords, each of which correspond to one of the keywords defined in the JSON Schema specifications.

## Specification versions

There are currently six drafts of the JSON Schema specification that have known use:

- Draft 3
- Draft 4
- Draft 6
- Draft 7
- Draft 2019-09
- Draft 2020-12

JsonSchema.Net supports draft 6 and later.

The next version, which will be supported by v4.0.0 and later of this library, is currently in development and will start a new era for the project which includes various backward- and forward-compatibility guarantees.  Have a read of the various discussions happening in the [JSON Schema GitHub org](https://github.com/json-schema-org) for more information.

### Meta-schemas

Each version defines a meta-schema.  This is a special JSON Schema that describes all of the keywords available for that version.  They are intended to be used to validate other schemas.  Usually, a schema will declare the version it should adhere to using the `$schema` keyword.

JsonSchema.Net declares the meta-schemas for the supported versions as members of the `MetaSchemas` static class.

Draft 2019-09 introduced vocabularies.  As part of this new feature, the meta-schemas for this version and those which follow it have been split into vocabulary-specific meta-schemas.  Additionally, the specification recognizes that the meta-schemas aren't perfect and may need to be updated occasionally.  As such, the meta-schemas defined by this library will be updated to match, in most cases only triggering a patch release.

## Keywords

JSON Schema is expressed as a collection of keywords, each of which provides a specific constraint on a JSON instance.  For example, the `type` keyword specifies what JSON type an instance may be, whereas the `minimum` keyword specifies a minimum numeric value *for only numeric data* (it will not apply any assertion to non-numeric values).  These keywords can be combined to express the expected shape of any JSON instance.  Once defined, the schema evaluates the instance, providing feedback on what errors occurred, including where in the instance and in the schema produced them.

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

### Easy mode

Some of the more straightforward builder methods are for like the `title` and `$comment` keywords, which just take a string:

```c#
builder.Comment("a comment")
    .Title("A title for my schema");
```

Notice that these methods implement a fluent interface so that you can chain them together.

### A little spice

Other extension methods can take multiple values.  These have been overloaded to accept both `IEnumerable<T>` and `params` arrays just to keep things flexible.

```c#
var required = new List<string>{"prop1", "prop2"};
builder.Required(required);
```

or just

```c#
builder.Required("prop1", "prop2");
```

### Now you're cooking with gas

Lastly, we have the extension methods which take advantage of C# 7 tuples.  These include keywords like `$defs` and `properties` which take objects to mimic their JSON form.

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

# Evaluation & annotations

***NOTE** In recognizing the multitude of uses for JSON Schema, the team has started to use the word "evaluate" instead of "validate" for the general processing of a schema.  What was "validate" in v3.x of this library is now "evaluate" in order to align with this viewpoint.*

## Evaluating instances

`JsonSchema` exposes an `Evaluate()` method which is used to evaluate JSON instances.  Let's begin with the following schema and a few JSON objects:

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

To evaluate these, all we have to do is pass these into our schema's `Evaluate(JsonElement)` method.

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

var emptyResults = schema.Evaluate(emptyJson);
var booleanResults = schema.Evaluate(booleanJson);
var stringResults = schema.Evaluate(stringJson);
var shortResults = schema.Evaluate(shortJson);
var numberResults = schema.Evaluate(numberJson);
var nonObjectResults = schema.Evaluate(nonObject);
```

The various results objects are of type `EvaluationResults`.  More information about the results object can be found in the next section.

In the above example, the following would result:

- `emptyJson` and `numberJson` failed because `"myProperty"` was not found.
- `booleanJson` failed because the value of `"myProperty"` is of the wrong type.
- `stringJson` passed validation.
- `shortJson` failed because the value of `"myProperty"` was too short.
- `nonObject` also passed validation because `properties` and `required` ignore non-object JSON.

No errors would actually be reported here because the output format defaults to a "flag" format, which is a basic pass/fail.  To get specific errors, the output format will need to be configured.

## Evaluation results

JSON Schema draft 2019-09 began the process to standardize the format for evaluation output in order to support cross-platform and cross-implementation compatibility.  The format has been updated for the upcoming release to be more concise and clear.  This includes support for both errors and annotation collection.

In summary, there are three levels of verbosity for output: Flag, List, and Hierarchy.

The flag format will simply return a boolean value indicating a pass/fail result.  All other formats include JSON Pointers and URIs to indicate the source of the errors or annotations that were produced.

A list format reduces all of the errors to a flat list, housed in a top-level object that summarizes the validation result.

The hierarchical output format follows the structure of the schema.

The default output format is Flag, but this can be configured via the `EvaluationOptions.OutputFormat` property.

***NOTE** It's only possible to translate from a more detailed to a less detailed format.*

### Examples of output

<details>
  <summary>Hierarchical</summary>

```json
{
  "valid": false,
  "evaluationPath": "",
  "schemaLocation": "https://json-schema.org/schemas/example#",
  "instanceLocation": "",
  "details": [
    {
      "valid": false,
      "evaluationPath": "/properties/foo",
      "schemaLocation": "https://json-schema.org/schemas/example#/properties/foo",
      "instanceLocation": "/foo",
      "details": [
        {
          "valid": false,
          "evaluationPath": "/properties/foo/allOf/0",
          "schemaLocation": "https://json-schema.org/schemas/example#/properties/foo/allOf/0",
          "instanceLocation": "/foo",
          "errors": {
            "required": "Required properties [\"unspecified-prop\"] were not present"
          }
        },
        {
          "valid": false,
          "evaluationPath": "/properties/foo/allOf/1",
          "schemaLocation": "https://json-schema.org/schemas/example#/properties/foo/allOf/1",
          "instanceLocation": "/foo",
          "droppedAnnotations": {
            "properties": [ "foo-prop" ],
            "title": "foo-title"
          },
          "details": [
            {
              "valid": false,
              "evaluationPath": "/properties/foo/allOf/1/properties/foo-prop",
              "schemaLocation": "https://json-schema.org/schemas/example#/properties/foo/allOf/1/properties/foo-prop",
              "instanceLocation": "/foo/foo-prop",
              "errors": {
                "const": "Expected \"1\""
              },
              "droppedAnnotations": {
                "title": "foo-prop-title"
              }
            },
            {
              "valid": true,
              "evaluationPath": "/properties/foo/allOf/1/additionalProperties",
              "schemaLocation": "https://json-schema.org/schemas/example#/properties/foo/allOf/1/additionalProperties",
              "instanceLocation": "/foo/other-prop"
            }
          ]
        }
      ]
    },
    {
      "valid": false,
      "evaluationPath": "/properties/bar",
      "schemaLocation": "https://json-schema.org/schemas/example#/properties/bar",
      "instanceLocation": "/bar",
      "details": [
        {
          "valid": false,
          "evaluationPath": "/properties/bar/$ref",
          "schemaLocation": "https://json-schema.org/schemas/example#/$defs/bar",
          "instanceLocation": "/bar",
          "droppedAnnotations": {
            "properties": [ "bar-prop" ],
            "title": "bar-title"
          },
          "details": [
            {
              "valid": false,
              "evaluationPath": "/properties/bar/$ref/properties/bar-prop",
              "schemaLocation": "https://json-schema.org/schemas/example#/$defs/bar/properties/bar-prop",
              "instanceLocation": "/bar/bar-prop",
              "errors": {
                "minimum": "2 is less than or equal to 10"
              },
              "droppedAnnotations": {
                "title": "bar-prop-title"
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
  <summary>List</summary>

```json
{
  "valid": false,
  "details": [
    {
      "valid": false,
      "evaluationPath": "/properties/foo/allOf/0",
      "schemaLocation": "https://json-schema.org/schemas/example#/properties/foo/allOf/0",
      "instanceLocation": "/foo",
      "errors": {
        "required": "Required properties [\"unspecified-prop\"] were not present"
      }
    },
    {
      "valid": false,
      "evaluationPath": "/properties/foo/allOf/1/properties/foo-prop",
      "schemaLocation": "https://json-schema.org/schemas/example#/properties/foo/allOf/1/properties/foo-prop",
      "instanceLocation": "/foo/foo-prop",
      "errors": {
        "const": "Expected \"1\""
      }
    },
    {
      "valid": false,
      "evaluationPath": "/properties/bar/$ref/properties/bar-prop",
      "schemaLocation": "https://json-schema.org/schemas/example#/$defs/bar/properties/bar-prop",
      "instanceLocation": "/bar/bar-prop",
      "errors": {
        "minimum": "2 is less than or equal to 10"
      }
    }
  ]
}
```

</details>

## Value format validation

The `format` keyword has been around a while.  It's available in all of the versions supported by JsonSchema.Net.  Although this keyword is technically classified as an annotation, the specification does allow (the word used is "SHOULD") that implementation provide some level of validation on it so long as that validation may be configured on and off.

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

The `EvaluationOptions` class gives you a few configuration points for customizing how the evaluation process behaves.  It is an instance class and can be passed into the `JsonSchema.Evaluate()` method.  If no options are explicitly passed, a copy of `JsonSchemaOptions.Default` will be used.

- `EvaluateAs` - Indicates which schema version to process as.  This will filter the keywords of a schema based on their support.  This means that if any keyword is not supported by this version, it will be ignored.
- `EvaluateMetaSchema` - Indicates whether the schema should be evaluated against its `$schema` value (its meta-schema).  This is not typically necessary.  Note that the evaluation process will still attempt to resolve the meta-schema. \*
- `OutputFormat` - You already read about output formats above.  This is the property that controls it all.  By default, a single "flag" node is returned.  This also yields the fastest evaluation times as it enables certain optimizations.
- `ProcessCustomKeywords` - For schema versions which support the vocabulary system (i.g. 2019-09 and after), allows custom keywords to be processed which haven't been included in a vocabulary.  This still requires the keyword type to be registered with `SchemaRegistry`.

_\* If you're using a custom meta-schema, you'll need to load it per the [Schema Registration](json-schema#schema-registration) section below.  Custom meta-schemas form a chain of meta-schemas (e.g. your custom meta-schema may reference another which references the draft 2020-12 meta-schema).  Ultimately, the chain MUST end at a JSON-Schema-defined meta-schema as this defines the processing rules for the schema.  An error will be produced if the meta-schema chain ends at a meta-schema that is unrecognized._

# Managing references (`$ref`)

JsonSchema.Net handles all references as defined in the draft 2020-12 version of the JSON Schema specification.  What this means for draft 2019-09 and later schemas is that `$ref` can now exist alongside other keywords; for earlier versions, keywords as siblings to `$ref` will be ignored.

## Schema registration

In order to resolve references more quickly, JsonSchema.Net maintains two schema registries for all schemas and subschemas that it has encountered.  The first is a global registry, and the second is a local registry that is passed around on the evaluation context.  If a schema is not found in the local registry, it will automatically fall back to the global registry.

A `JsonSchema` instance will automatically register itself upon calling `Evaluate()`.  However, there are some cases where this may be insufficient.  For example, in cases where schemas are separated across multiple files, it is necessary to register the schema instances prior to evaluation.

For example, given these two schemas

```json
{
  "$id": "http://localhost/my-schema",
  "type": "object",
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

You must register `random-string` before you attempt to evaluate with `my-schema`.

```c#
var randomString = JsonSchema.FromFile("random-string.json");
SchemaRegistry.Global.Register("http://localhost/random-string", randomString);
```

Now JsonSchema.Net will be able to resolve the reference.

## Automatic resolution

In order to support scenarios where schemas cannot be registered ahead of time, the `SchemaRegistry` class exposes the `Fetch` property which is defined as `Func<Uri, JsonSchema>`.  This property can be set to a method which downloads the content from the supplied URI and deserializes it into a `JsonSchema` object.

The URI that is passed may need to be transformed, based on the schemas you're dealing with.  For instance if you're loading schemas from a local filesystem, and the schema `$ref`s use relative paths, you may need to prepend the working folder to the URI in order to locate it.

# Customizing error messages

The library exposes the `ErrorMessages` static type which includes read/write properties for all of the error messages.  Customization of error messages can be achieved by setting these properties.

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