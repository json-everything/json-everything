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

If the C# is valid, you will receive a JSON Schema in the results editor.  Otherwise, you will
receive an error that says ""compilation error.""

***NOTE** The playground will output the compilation error to the console, including the
serialized details. I haven't yet figured out how to interpret these details to provide any
insight into what in particular is wrong.  If you know, please feel free to raise an issue.*";

	public const string SchemaGenerateCode = @"
## Generating C# Code

The schema for which you want to generate code will be placed in the editor on the left.

The editor on the right is unused.

Click ""Run"".

Your code (or an error message) will be output in the bottom editor.

Please be sure to read the [documentation](https://docs.json-everything.net/schema/codegen/schema-codegen/)
to see what is supported.";

	public const string SchemaGenerateData = @"
## Generating Random Data

Enter the schema for which you like to generate data into the left editor.

Click ""Run"", and the resulting data will appear in the right editor.

There are a few limitations on which keywords are supported for data generation.  Please
read the [documentation](https://docs.json-everything.net/schema/datagen/schema-datagen/) to find out
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

	public const string SchemaUnknownKeywordsAnnotation = @"
(experimental) Adds an annotation to the output that contains a list of schema keywords that were
unrecognized by its meta-schema.  See
[this JSON Schema discussion](https://github.com/orgs/json-schema-org/discussions/512) for more
information to provide feedback on the potential feature.";

	public const string SchemaValidateFormat = @"
Allows you to specify whether the `format` keyword should be asserted.  Typically this is an
annotation-only keyword, meaning that it will appear in the output, but it will not be validated.";

	public const string PathExtraWhitespace = @"
JSON Path only permits whitespace in specific locations.  This option will allow whitespace everywhere.
";

	public const string PathJsonLiterals = @"
JSON Path doesn't allow composite JSON literals such as arrays and objects in filter expressions.
This option will allow these kinds of values.  For example,

```
$[?@.foo=={""key"":""value""}]
```

to find all elements where `@.foo` is the JSON object `{""key"":""value""}`.

Composite JSON literals are only valid for equality checks, not comparisons.

JSON literals MUST use double quotes `""`, not single quotes `'`.";

	public const string PathJsonInOperator = @"
JSON Path does not support the `in` operator in expressions.  For example,

```
$[?@.foo in [""a"",""b"",""c""]]
```

to find all elements where `@.foo` is either `""a""`, `""b""`, or `""c""`.  Or

```
$[?!(5 in @.bar)]
```

to find all elements where `@.bar` does not contain a `5`.
";

	public const string PathMathOperators = @"
JSON Path only defines equality and comparison operators.  This enables other math operators.
Unless otherwise specified, these are only valid for numeric values.  If an incompatible value is
encountered during evaluation, the node will not be selected.

- `+` - addition (numbers) & concatenation (strings)
- `-` - subtraction
- `*` - multiplication
- `/` - division
";

	public const string PathRelativeStart = @"
JSON Path requires paths to start with `$`.  However, during development of the specification,
a use case was raised for allowing paths to start with `@`.  This option allows such paths.
For example, `@.foo` would be valid as a complete path.
";

	public const string PathEvaluate = @"
By default, _JsonPath.Net_ operates per the specification.  The above options allow
the library to deviate from the specification in useful ways.
";

	public const string JsonEEvaluate = @"
JSON-e evaluates templates with an optional context.

The template may be any JSON value, and the context must be an object.  The output will be displayed below.

Please read the [documentation](https://docs.json-everything.net/json-e/basics/) for more information.

It may also be beneficial to compare your results with the [original JavaScript playground](https://json-e.js.org/playground.html).
";

	public const string PatchApply = @"
To apply a patch, enter the patch in the left editor and the data to patch in the right editor.
";

	public const string PatchGenerate = @"
To generate a patch, enter the starting data in the left editor and the target data in the right editor.

Patch generation follows an iterative strategy, preferring multiple small changes.  This may result in
larger patches than expected.
";
}