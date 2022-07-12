# Working With Options

There are a few objects which declare static default values:

- `ValidationOptions`
- `SchemaRegistry`
- `VocabularyRegistry`

***NOTE** The second two are properties on the `ValidationOptions`.*

All of these objects are also instance objects which can be configured independently of the defaults.  When a schema validation begins, it makes copies of everything and runs from the copies.  This means that every validation run can be completely isolated if needed.

Additionally, for the two registries, the default instances are used as fallbacks for when the requested value can't be found in the local instance.

Let's look at this a bit more deeply.

## Setting output format

To set `Detailed` as the default output format for _all_ validations, use the default instance:

```c#
ValidationOptions.Default.OutputFormat = OutputFormat.Detailed;
```

However if you want to then get a `Verbose` output for just _one_ validation run, you can create a new options object and pass this into the `.Validate()` call.

```c#
var options = new ValidationOptions { OutputFormat = OutputFormat.Verbose };
var results = schema.Validate(instance, options);
```

When you create a new options object, it copies all of the values from the default instance (`ValidationOptions.Default`) as a starting point.  From there, you can make changes to your instance without affecting other validations.

`SchemaRegistry` and `VocabularyRegistry` options work a bit differently, though.

## Configuring SchemaRegistry and VocabularyRegistry

The default instance for these objects is actually called `Global` because they serve as a fallback for when the local instances can't find what's being requested.  Let's look at preloading schemas to see how the schema vocabulary manages a local registry against the global one.

***ASIDE** `VocabularyRegistry` works exactly the same, so we're only going to discuss one.*

As mentioned in the [Handling Externally-defined Schemas](#handling-externally-defined-schemas) example, to reference external schemas, they need to be preloaded first.  That example shows simply adding the schemas to the global registry.  This makes the schema available to _all_ validations.

```c#
SchemaRegistry.Global.Register(schema);
```

When the validation runs, a new, empty registry is created and set onto the options.  When the validator encounters a `$ref` keyword with a URI for an external document, it calls `options.SchemaRegistry.Get()` to retrieve the referenced schema.  Note that this is the options objects local copy.  Since you haven't set anything there, nothing is found.  So it then checks the global registry and finds it.

It's doing this for you:

```c#
return localRegistry.Get(uri) ?? globalRegistry.Get(uri);
```

As mentioned, registering the schema with the global registry makes the schema available to everyone because of this fallback logic. To only make the schema availble to a single validation, you'll need to register with the local registry.  This must be done through the options object prior to validation.

```c#
var options = new ValidationOptions();
options.SchemaRegistry.Register(externalSchema);

var results = schema.Validate(instance, options);
```

Now, the local registry will find something, and it won't fall back to the global.  Moreover, your external schema is only available for this validation (or any validation that uses this options object).