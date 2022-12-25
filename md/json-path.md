***NOTE** JsonPath.Net needs a larger rewrite in order to support `JsonNode` properly.  While the other libraries have been converted already, this one will take a bit longer.*

JSON Path is a query language for JSON documents inspired by what XPath provides for XML documents.  It was [originally proposed](https://goessner.net/articles/JsonPath/) by Matt Goessner, and now a [specification](https://github.com/jsonpath-standard/internet-draft) is in progress.

Version 0.3.x is aligned with the specification as of the end of 2022.  It also supports functions (which has an [open PR](https://github.com/ietf-wg-jsonpath/draft-ietf-jsonpath-base/pull/330)) and arithmetic operations in expressions (not part of the spec yet).

## Syntax

A path consists of start indicator followed by a series of segments, chained one after another.  Each segment contains one or more selectors.  Each selector takes in a collection of JSON nodes and produces a collection of JSON nodes (called a "nodelist") based on their function.  The output of the segment is the collective output of all of that segment's selectors.  Each segment takes as input the output of the previous selector.

- `$` - This is the root.  A path must always begin with it.  It effectively "selects" the root document.  It can also be used in query expressions, which we'll come to later.
- `[...]` - Square brackets indicate a segment.  Selectors are found between the brackets, separated by commas.  There are several kinds of selectors:
  - Index - The basic integer index that we're all familiar with.  Negative numbers will start counting from the end of an array.  If the value is out of the range of the array, no nodes will be returned.
  - [Slice](https://stackoverflow.com/a/509295/878701) - This allows selection of a range of nodes from an array.  Again, this range is clamped to the bounds of the array.
  - Property name - This allows property selection of objects by matching the full name.  Names can be single- or double-quoted and special characters must be encoded as is required by JSON.
  - Wildcard (`*` literal) - This simply returns all values from within an object or array.
  - Filter - This is an expression that evaluates to a boolean and operates on the child nodes of the node being passed to the selector.  It is denoted by a question mark followed an expression `?...` that returns a boolean result.\*
- `..` - This is a recursive descent operator.  It is not itself a segment, but a segment prepended with this operator will recursively query the entire subtree rather than just the local value.

\* _**NOTE** This boolean result is distinct from a JSON boolean, which is denoted by either the `true` or `false` literals.  Using `true` or `false` in an expression is interpreted as a JSON literal and must be used in a comparison._

In addition to the above, there are a few shorthand options for some special cases.   These are only valid when the segment contains only a single selector and that selector is either a property name or a wildcard.

- `['foo']` may be rewritten as `.foo`.
- `[*]` may be rewritten as `.*`.
- `..['foo']` may be rewritten as `..foo`
- `..[*]` may be rewritten as `..*`

### Query Expressions

Filter selectors take an expression.  This expression uses a single variable, which is a JSON node as described above.  The node is denoted by `@` and any valid JSON Path can follow it.  The `@` is a stand-in for the `$` from above and acts as the root of the local value.

For example, an item query expression may be `?@.price<10`.  This expression will find all items in either an object or an array that contains a `price` property with a value less than 10.

Additionally, the `$` selector may also be used to reference back to the root node.  This allows queries like `?@.price<$.maxPrice` where we want to find all of the items of the current container that contain a `price` value that is less than the value in the root node's `maxPrice` property.

_**TIP** This library considers paths that start with `$` to be "globally scoped" and paths that start with `@` to be "locally scoped."_

Expressions support the following operations:

- Arithmetic
  - `+`
  - `-`
  - `*`
  - `/`
  <!-- - `%` (modulus) -->
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

_**NOTE** Arithmetic operations are not part of the specification (yet), and cannot be expected to work in other JSON Path implementations._

#### Functions

There is also support for functions, which works as an extension point to add your own custom logic.

A function is a name followed by a parentheses containing zero or more parameters separated by commas.  Parameters can be JSON literals, JSON Paths (global or local), or even other functions.  (That is, the return value of function calls can be parameters, e.g. `min(max(@,0),10)`; passing one function into another isn't supported.)

The specification defines the following functions:

- `length(<value>)` to return the length of a string or the number of items in an array or object.  Takes a single parameter.
- `count(<nodelist>)` to return the number of nodes in a nodelist.
- `match(<iregexp>, <text>)` to return whether the text is an exact match for a regular expression per [I-Regexp](https://www.ietf.org/archive/id/draft-ietf-jsonpath-iregexp-02.html).\*
- `search(<iregexp>, <text>)` to return whether the text _contains_ an exact match for a regular expression per [I-Regexp](https://www.ietf.org/archive/id/draft-ietf-jsonpath-iregexp-02.html).\*

\* _I-Regexp is designed to be an interoperable subset of most popular regular expression specifications and implementations.  One difference that [could not be resolved](https://github.com/ietf-wg-jsonpath/iregexp/issues/15) was implicit anchoring.  As such, two methods were developed to handle both cases.  `match` uses implicit anchoring, while `search` does not._

## In Code

To obtain an instance of a JSON Path, you'll need to parse it from a string.

```c#
var path = JsonPath.Parse("$.prop[0:6:2]");
```

or

```c#
var success = JsonPath.TryParse("$.prop[0:6:2]", out JsonPath path);
```

This will create a `JsonPath` instance that will select the `prop` property of an object, and subsequently items, 0, 2, and 4 of an array that resides there.

Once your path is created, you can start evaluating instances.

```c#
var instance = JsonNode.Parse("{\"prop\":[0,1,2,3]}");

var results = path.Evaluate(instance);
```

This will return a results object that contains the resulting nodelist or an error.

A node contains both the value that was found and the location in the instance _where_ it was found.  The location is always represented using the "canonical," bracketed format.

## Adherence to the Proposed Specification

As the specification is still under authorship, there are features present in traditional JSON Path that haven't been properly described yet.  For these features, this library has been configured to mimic the consensus behaviors of other libraries as determined by the [JSON Path Comparison](https://cburgmer.github.io/json-path-comparison/) project.

There are also a few other features of traditional JSON Path that the specification has explicitly elected _not_ to support, such as container expressions (e.g. `$[(@.length-1)]`).  This library will strive to prioritize the specification over the comparison consensus where any conflict exists.
