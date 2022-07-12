# Handling Externally-defined Schemas

JSON Schema has multiple ways to reference other schemas.  This is done to both reduce the size of the schemas that we humans have to deal with as well as to promote code reuse.  Defining a schema once to be used in multiple places is often a better approach than rewriting it in all of those places.  It also allows us to define recursive schemas.

References typically come in two flavors: internal and external.  Internal references can be identified by the `$ref` keyword along with a fragment-only URI in the form of a JSON Pointer, such as `#/$defs/foo`.  However, it's common to define your schemas across several files.  These are external references and the value for `$ref` in these cases needs to indicate the file, e.g. `https://my-example.org/schema`.

***NOTE** You _can_ reference a subschema of an external document by giving the patht to the file and a pointer to a subschema, but this isn't usually done.  Instead, that subschema would be extracted into its own file to be referenced by both schemas.*

## Resolving references

JsonSchema.Net will automatically handle internal references.  The schema document is loaded, and the library can easily resolve pointers inside of it.

In order for JsonSchema.Net to handle external schemas, however, the schemas must be loaded and registered before validation starts.

Suppose you have a subfolder where you store your schema files.  To load them, just iterate through the files and register them with `SchemaRegistry`.

```c#
var files = Directory.GetFiles("my-schemas", "*.json");
foreach (var file in files)
{
    var schema = JsonSchema.FromFile(file);
    SchemaRegistry.Global.Register(schema);
}
```

It's best practice to ensure all of your schemas declare an `$id` keyword at their root.  If a schema doesn't have this keyword, `FromFile()` will automatically assign the `file:///` URI of the full file name in order to reference this schema.

`SchemaRegistry.Global.Register()` is the part that matters here.  This adds the schema to the internal registry so that, when the schema is needed, it can be found.

## Dynamically loading references

An alternative to preloading schemas is setting up an automatic download by setting the `SchemaRegistry.Default.Fetch` function property.

***DISCLAIMER** Automatically downloading external data is [explicitly recommended against](https://json-schema.org/draft/2020-12/json-schema-core.html#name-schema-references) by the specficiation.  This functionality is added for convenience and disabled by default.*

```c#
JsonSchema? DownloadSchema(Uri uri)
{
    try
    {
        var content = new HttpClient().GetStringAsync(uri).Result;
        return JsonSchema.FromText(content);
    }
    catch (Exception e)
    {
        return null;
    } 
}

SchemaRegistry.Global.Fetch = DownloadSchema;
```

To clear the download function, simply set `null`.  The property isn't declared as nullable, but this will reset the property to a function that just returns null.

```c#
SchemaRegistry.Global.Fetch = null!;
```