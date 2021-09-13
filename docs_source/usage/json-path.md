# JsonPath<nsp>.Net

JSON Path is a query language for JSON documents inspired by what XPath provides for XML documents.  It was [originally proposed](https://goessner.net/articles/JsonPath/) by Matt Goessner, and now a [specification](https://github.com/jsonpath-standard/internet-draft) is in progress.

<p style="text-align: center;">
<a href="https://json-everything.net/json-path" target="_block" style="color: rgb(255, 255, 255); background-color: rgb(13, 71, 161); display: inline-block; font-weight: 500; font-size: 2rem; text-align: center; vertical-align: middle; padding: 0.6rem 0.9rem; border-radius: 0.35rem; cursor: pointer; user-select: none; text-decoration: none; --darkreader-inline-color:#ffffff; --darkreader-inline-bgcolor:#06419c; margin-top: 1.5rem !important;">Try it online!</a>
</p>

## Syntax

A path consists of a series of selectors, chained one after another.  Each selector takes in a collection of JSON nodes and produces a collection of JSON nodes based on their function.

- `$` - This is the root selector.  A path must always begin with it.  It returns the root document.  It can also be used in query expressions, which we'll come to later.
- `.` - This is a property-name selector.  The property names that you can use with this must only contain alphanumeric or underscore `_` characters.  Otherwise you'll need to use the property-name indexer (see below).  This selector also provides a recursive-descent function when immediately followed by another selector.  (More on recursive descents below.)  The property name can be replaced with an asterisk `*` to indicate that all properties should be returned.
- `[]` - This is an indexing selector.  It can contain multiple indices, separated by commas.  Indices come in several flavors:
    - Simple integer - The basic integer index that we're all familiar with.  Negative numbers will start counting from the end of an array.  If the value is out of the range of the array, no nodes will be returned.
    - [Slice](https://stackoverflow.com/a/509295/878701) - This allows selection of a range of nodes from an array.
    - Property name - This allows property selection of objects by matching the full name.  This is required when a property key doesn't match the requirements of the `.` selector from above.  Keys can be single- or double-quoted and special characters must be encoded as is required by JSON.
    - `*` literal - This is a wildcard that simply returns all values from within the object or array.
    - Container query - This is an expression that evaluates to a single integer index and operates on the node being passed to the selector.  It is denoted by parentheses surrounding the expression `(...)`.
    - Item query - This is an expression that evaluates to a boolean and operates on the child nodes of the node being passed to the selector.  It is denoted by a question mark followed by parentheses surrounding the expresssion `?(...)`.

### Query Expressions

Both container queries and item queries take an expression.  This expression uses a single variable, which is some JSON node as described above.  The node is denoted by `@` and any valid JSON Path can follow it.

For example, an item query expression may be `?(@.price<10)`.  This expression will find all items in either an object or an array that contains a `price` property with a value less than 10.

Additionally, the `$` selector may be used to reference back to the root node, even inside an expression.  This allows queries like `?(@.price<$.maxPrice)` where we want to find all of the items of the current container that contain a `price` value that is less than the value in the root node's `maxPrice` property.

Exressions support the following operations:

- Arithmetic
    - `+`
    - `-`
    - `*`
    - `/`
    - `%` (modulus)
- Comparison
    - `==`
    - `!=`
    - `<`
    - `<=`
    - `>`
    - `>=`
- Boolean logic
    - `&&`
    - `||`

***NOTE** There is an [open issue](https://github.com/jsonpath-standard/internet-draft/issues/17) regarding expression support on the specification GitHub repository.

### Recursive Descents

When the `.` selector is immediately followed by another selector (`.` or `[]`), the initial `.` indicates a recursive descent selector.  This will return _all_ nodes recursively.  So for `{ "a": [ 1, 2, 3 ] }` it will return all of the following:

- `{ "a": [ 1, 2, 3 ] }`
- `[ 1, 2, 3 ]`
- `1`
- `2`
- `3`

### Special Considerations

There is also support for a special-case `.length` property, which will return the number of items within an object or array.  It is valid on both the base path as well as paths found in expressions.

If you find that you need the value of a `length` property, you'll need to use the indexer syntax `["length"]`.

This allows expressions such as the container expression `(@.length-1)` which is equivalent to the integer index `-1` and returns the last value in an array.

## In Code

To obtain an instance of a JSON Path, you'll need to parse it from a string.

```c#
var path = JsonPath.Parse("$.prop[0:6:2]");
```

or

```c#
var success = JsonPath.TryParse("$.prop[0:6:2]", out JsonPath path);
```

This will create a `JsonPath` instance that will select the `prop` property of an object, then items, 0, 2, and 4 of an array that resides there.

Once your path is created, you can start evaluating instances.

```c#
var instance = JsonDocument.Parse("{\"prop\":[0,1,2,3]}");

var results = path.Evaluate(instance);
```

This will return a results object that contains an array of matches or an error.

The match object contains both the value that was found and the location in the instance _where_ it was found.

## Adherence to the Proposed Specification

As the specification is still under authorship, there are features present in traditional JSON Path that haven't been properly described yet.  For these features, this library has been configured to mimic the consensus behaviors of other libraries as determined by the [JSON Path Comparison](https://cburgmer.github.io/json-path-comparison/) project.

This library will strive to prioritize the specification over the comparison consensus where any conflict exists.
