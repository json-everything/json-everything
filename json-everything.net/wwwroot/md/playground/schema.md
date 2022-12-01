# Using the Playground

This playground supports three modes of operation: validation, schema generation, and data generation.  What to put where, configuration of options, and what to click depends on what you want to do.

Keep reading for how to use each.

## Validation

This is the primary mode for the playground.

To use this mode effectively, ensure the _Language_ selection is set to JSON.  The playground's validation functionality is not dependent upon this setting, but it will set up the input editor's syntax highlighting, making editing easier.

The schema to be used for validation will go in the editor on the left.  The JSON data to validate will go in the editor on the right.

There are a number of settings available in the upper portion of the middle area.

|Setting|Description|
|:-|:-|
|Draft|Allows selection of which JSON Schema version to use.  This option serves as a fallback for when the schema itself does not include a `$schema` keyword.|
|Output format|Allows selection of the output format.  This format is described in the JSON Schema specification.  JsonSchema.Net uses the format specified in version 2020-12, however this playground uses the proposal for the next draft, which no longer contains a "condensed" hierarchy format.  See the output format section of the docs (to the left) for more information on what the library offers.|
|Output structure|The next version of JSON Schema is changing the output structure from being keyword-based to being subschema-based.  The key difference is a more compact representation that consolidates annotations and errors in a more consumable format.  Try both to see which you prefer.|
|Preserve dropped annotations|Subschemas which fail validation must drop their annotations.  This option will retain them in the "draft-next" output under `droppedAnnotations`.  This can be useful for debugging.
|Default base URI|Allows you to specify a default base URI for the schema.  This serves as a fallback for when the `$id` keyword is not an absolute URI.  This is typically used in the output but also comes into play during `$ref` resolution.  The default is `https://json-everything.net/`|
|Validate `format`|Allows you to specify whether the `format` keyword should be asserted.  Typically this is an annotation-only keyword, meaning that it will appear in the output, but it will not be verified.|

Once your schema and data are in place, and you've selected your options, click the _Validate_ button, and the result will appear in the bottom editor.

## Schema Generation

To generate a schema from C# code, first choose C# from the _Language_ dropdown.  This will configure the editor on the left to highlight C# code and set up the generation button for schema generation.

Secondly enter your C# classes.  The playground will generate a schema for the first type it encounters.

There is no need to add a namespace, however you may need to add `using` statements depending on the types you expect to use.  A few are included by default:

- `System`
- `System.Collections.Generic`
- `System.Text.Json`
- `System.Text.Json.Nodes`
- `System.Text.Json.Serialization`
- `Json.Schema`
- `Json.Schema.Generation`

When you're done, click the _Generate schema_ button.

If the C# is valid, you will receive a JSON Schema in the right editor.  Otherwise, you will receive an error that says "compilation error."

***NOTE** The playground will output the error to the console, including the serialized details.  I haven't yet figured out how to interpret these details to provide any insight into what in particular is wrong.  If you know, please feel free to raise an issue.*

## Data Generation

To generate valid JSON data from a schema, set the _Language_ dropdown to JSON to configure the syntax highlighting and the generation button.

The schema for which you like to generate data will go in the left editor.

When ready, click the _Generate data_ button, and the resulting data will appear in the right editor.

There are a few limitations on which keywords are supported for data generation.  Please read the documentation available in the menu to the left to find out what's supported.