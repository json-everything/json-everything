# JsonLogic

> Not Json<nsp>_Something_<nsp>.Net, sadly.  "JsonLogic<nsp>.Net" was already registered on Nuget.  It breaks the pattern, I know.  _C'est la vie_.

[JsonLogic](https://jsonlogic.com) is a mechanism that can be used to apply logical transformations to JSON values and that is also itself expressed in JSON.

## The syntax

JsonLogic is expressed using single-keyed objects called _rules_.  The key is the operator and the value is (usually) an array containing the parameters for the operation.  Here are a few examples:

- Less than: `{"<" : [1, 2]}`
- Merging arrays: `{"merge":[ [1,2], [3,4] ]}`
- Value detection: `{"in":[ "Ringo", ["John", "Paul", "George", "Ringo"] ]}`

***NOTE** For rules that only have one parameter, that parameter can be expressed directly instead of in an array.  This shorthand is provided as a syntactic sugar.*

While explicitly listing rules is all well and good, the real power of this comes from the ability to reference input data using the `var` operator.  This operator
takes a dot-delimited path that is evaluated on the input object, and the `var` rule is replaced by the resolved value.

So if we want to ensure a value in the input data is less than 2, we could use `{"<": [{"var": "foo.bar"}, 2]}`.  This checks the input value for a `foo` then a `bar` property, returns that value, and compares it against 2.

There are many operators that work on different data types, ranging from string and array manipulation to arithmetic to boolean logic.  The full list is [on their website](https://jsonlogic.com/operations.html), and their docs are pretty good, so I won't repeat the list here.

## In code

The library defines an object model for rules, starting with the `Rule` base class.  This type is fully serializeable, so if you have rules in a text format, just deserialize them to get a `Rule` instance.

```c#
var rule = JsonSerializer.Deserialize<Rule>("{\"<\" : [1, 2]}");
```

Once you have a rule instance, you can apply it using the `.Apply()` method, which takes a `JsonElement` for the data.  Sometimes, you may not have a data instance; rather you just want the rule to run.  In these cases you can call `.Apply()` passing a `null` or by using the `.Apply()` extension method which takes no parameters.

```c#
var data = JsonDocument.Parse("{\"foo\": \"bar\"}").RootElement;
var result = rule.Apply(data);
```

In addition to reading and deserializing rules, you can define them inline using the `JsonLogic` static class.  This class defines methods for all of the built-in rules.

Creating the "less than" rule with a variable lookup from above:

```c#
var rule = JsonLogic.LessThan(JsonLogic.Variable("foo.bar"), 2);
```

The `2` here is actually implicitly cast to a `LiteralRule` which is a stand-in for discrete JSON elements.  It can hold any JSON value, and there are implicit casts for numeric, string (null uses the `string` cast since it's the only nullable type), and boolean types, as well as `JsonElement`.  For arrays and objects, it's best to express the value in a string and use either the `JsonDocument.Parse` mechanism shown above or one of the `.AsJsonElement()` overloads from [Json.More](json-more.md).

## Value equality and type casting



## Creating new rules