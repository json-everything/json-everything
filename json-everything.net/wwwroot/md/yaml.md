Yaml2JsonNode is pretty simple: it converts models of YAML text that has been parsed by YamlDotNet into `JsonNode`s.  That's it.

## Why?

OpenAPI and indeed many JSON Schema documents are written in YAML because (I'm told) it's more human-readable than JSON.  Therefore it makes sense to support YAML in the various libraries.

However, just as we let .Net handle the JSON parsing, we will let someone else handle the YAML parsing for us.  Ideally, we'd want to parse directly into `JsonNode`, but conversion between models is also a very acceptable solution.

## Using the extensions

First, you'll need to parse the YAML.  Consult [their docs](https://github.com/aaubry/YamlDotNet/wiki), of course, but a simple way is:

```c#
var stream = new YamlStream();
stream.Load(new StringReader(yamlText));
```

Now the documents (yes, there could be multiple documents in a single parse) are stored in `stream.Documents`.

You can convert all of them:

```c#
var allTheNodes = stream.ToJsonNode();
```

or just the one you want:

```c#
var singleNode = stream.Documents[0].ToJsonNode();
```

If you have browsed down into their structure and you have a particular `YamlNode` you want converted:

```c#
var specificJsonNode = specificYamlNode.ToJsonNode();
```

There's really not much to this.  Just remember one method: `.ToJsonNode()`.  It'll take care of you.
