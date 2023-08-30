namespace JsonEverythingNet.Services;

public static class HelpContent
{
	public const string SchemaEvaluate = @"
## Evaluating a schema

The schema to be used for evaluation will go in the editor on the left.

The JSON data to evaluate (the ""instance"") will go in the editor on the right.

Ensure the options are as desired and click ""Run"".";

	public const string SchemaGenerateSchema = @"
## Generating a Schema

Enter your C# classes in the editor on the left.  The playground will generate a schema for
the first type it encounters, so supporting classes need to be placed at the bottom of the code.

There is no need to add a namespace, however you may need to add `using` statements depending
on the types you expect to use.  A few are included by default:

- `System`
- `System.Collections.Generic`
- `System.Text.Json`
- `System.Text.Json.Nodes`
- `System.Text.Json.Serialization`
- `Json.Schema`
- `Json.Schema.Generation`

When you're done, click ""Run"".

If the C# is valid, you will receive a JSON Schema in the right editor.  Otherwise, you will
receive an error that says ""compilation error.""

***NOTE** The playground will output the error to the console, including the serialized details.
I haven't yet figured out how to interpret these details to provide any insight into what in
particular is wrong.  If you know, please feel free to raise an issue.*";

	public const string SchemaGenerateCode = @"
## Generating C# Code

The schema for which you want to generate code will be placed in the editor on the left.

The editor on the right is unused.

Click ""Run"".

Your code (or an error message) will be output in the bottom editor.

Please be sure to read the [documentation](https://docs.json-everything.net/schema/schema-codegen/)
to see what is supported.";

	public const string SchemaGenerateData = @"
## Generating Random Data

Enter the schema for which you like to generate data into the left editor.

Click ""Run"", and the resulting data will appear in the right editor.

There are a few limitations on which keywords are supported for data generation.  Please
read the [documentation](https://docs.json-everything.net/schema/schema-datagen/) to find out
what's supported.";

	public const string SchemaVersion = @"
Allows selection of which JSON Schema version to use.  This option serves as a fallback for when
the schema itself does not include a `$schema` keyword.";

	public const string SchemaOutputFormat = @"
The output format is described in the JSON Schema specification.  By default, JsonSchema.Net
uses the formats described by the next version of the specification.  More information can be
found in [this blog post](https://json-schema.org/blog/posts/fixing-json-schema-output).

The available formats are:

- Flag - just a true/false indication of whether validation succeeded.
- List - All validation results are organized into a top-level list.
- Hierarchical - Validation results are organized into a hierarchy that mimics the schema structure.

If you prefer, you can choose the equivalent 2020-12 output formats using the **Output Structure**
option below.  See the [evaluation results](https://docs.json-everything.net/schema/basics/#schema-results)
section of the docs for more information on what the library offers.";

	public const string SchemaOutputStructure = @"
By default, JsonSchema.Net uses the format described by the next version of the specification.  This
option allows you to instead select draft 2020-12 output.";

	public const string SchemaDroppedAnnotations = @"
Per the JSON Schema specification, subschemas which fail evaluation must drop their annotations.
This option will include these annotations under a `droppedAnnotations` property when using the
""Draft-Next"" output. This can be useful for debugging.";

	public const string SchemaValidateFormat = @"
Allows you to specify whether the `format` keyword should be asserted.  Typically this is an
annotation-only keyword, meaning that it will appear in the output, but it will not be validated.";
}