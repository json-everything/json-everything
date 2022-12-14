# Working With Options

There are a few objects which declare static default values:

- `EvaluationOptions`
- `SchemaRegistry`
- `VocabularyRegistry`

***NOTE** The second two are properties on the `EvaluationOptions`.*

All of these objects are also instance objects which can be configured independently of the defaults.  When schema evaluation begins, it makes copies of everything and runs from the copies.  This means that every evaluation run can be completely isolated if needed.

Additionally, for the two registries, the default instances are used as fallbacks for when the requested value can't be found in the local instance.

Let's look at this a bit more deeply.

## Setting output format

To set `Detailed` as the default output format for _all_ evaluations, use the default instance:

```c#
EvaluationOptions.Default.OutputFormat = OutputFormat.Detailed;
```

However if you want to then get a `Verbose` output for just _one_ evaluation run, you can create a new options object and pass this into the `.Evaluate()` call.

```c#
var options = new EvaluationOptions { OutputFormat = OutputFormat.Verbose };
var results = schema.Evaluate(instance, options);
```

When you create a new options object, it copies all of the values from the default instance (`EvaluationOptions.Default`) as a starting point.  From there, you can make changes to your instance without affecting other evaluations.

`SchemaRegistry` and `VocabularyRegistry` options work a bit differently, though.

## Configuring SchemaRegistry and VocabularyRegistry

The default instance for these objects is actually called `Global` because they serve as a fallback for when the local instances can't find what's being requested.  Let's look at preloading schemas to see how the schema vocabulary manages a local registry against the global one.

***ASIDE** `VocabularyRegistry` works exactly the same, so we're only going to discuss one.*

As mentioned in the [Handling Externally-defined Schemas](#handling-externally-defined-schemas) example, to reference external schemas, they need to be preloaded first.  That example shows simply adding the schemas to the global registry.  This makes the schema available to _all_ evaluations.

```c#
SchemaRegistry.Global.Register(schema);
```

When the evaluation runs, a new, empty registry is created and set onto the options.  When the evaluator encounters a `$ref` keyword with a URI for an external document, it calls `options.SchemaRegistry.Get()` to retrieve the referenced schema.  Note that this is the options object's local copy.  Since you haven't set anything there, nothing is found.  So it then checks the global registry and finds it.

It's doing this for you:

```c#
return localRegistry.Get(uri) ?? globalRegistry.Get(uri);
```

As mentioned, registering the schema with the global registry makes the schema available to everyone because of this fallback logic. To only make the schema available to a single evaluation, you'll need to register with the local registry.  This must be done through the options object prior to evaluation.

```c#
var options = new EvaluationOptions();
options.SchemaRegistry.Register(externalSchema);

var results = schema.Evaluate(instance, options);
```

Now, the local registry will find something, and it won't fall back to the global.  Moreover, your external schema is only available for this evaluation (or any evaluation that uses this options object).