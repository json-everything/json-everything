# Extending JSON Schema Validation

These examples will show how to extend JSON Schema validation by creating a new keyword and incorporating it into a new vocabulary.

***NOTE** These examples are actually defined in one of the library's unit tests.*

For a more detailed explanation about the concepts behind vocabularies, please see the [Custom Keywords page](../usage/schema-keywords.md).

## Defining a Keyword

We want to define a new `maxDate` keyword that allows a schema to enforce a maximum date value to appear in an instance property.  We'll start with the keyword.

```c#
// The SchemaKeyword attribute is how the deserializer knows to use this
// class for the "maxDate" keyword.
[SchemaKeyword(Name)]
// Naturally, we want to be able to deserialize it.
[JsonConverter(typeof(MaxDateJsonConverter))]
class MaxDateKeyword : IJsonSchemaKeyword, IEquatable<MaxDateKeyword>
{
    // Define the keyword in one place.
    internal const string Name = "maxDate";

    // Define whatever data the keyword needs.
    public DateTime Date { get; }

    public MaxDateKeyword(DateTime date)
    {
        Date = date;
    }

    // Implements IJsonSchemaKeyword
    public void Validate(ValidationContext context)
    {
        // The value will come from the instance as a string,
        var dateString = context.LocalInstance.GetString();
        // but we want a date.
        var date = DateTime.Parse(dateString);

        // Check if the date is less than or equal to what we expect.
        if (date <= Date)
            // if so, pass validation.
            context.Pass();
        else
            // If not, fail validation and an error message.
            context.Fail($"{date:O} must be on or before {Date:O}")
    }

    // Equality stuff
    public bool Equals(MaxDateKeyword other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Date.Equals(other.Date);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as MaxDateKeyword);
    }

    public override int GetHashCode()
    {
        return Date.GetHashCode();
    }
}
```

We need to define that serializer, too.

```c#
class MaxDateJsonConverter : JsonConverter<MaxDateKeyword>
{
    public override MaxDateKeyword Read(ref Utf8JsonReader reader,
                                        Type typeToConvert,
                                        JsonSerializerOptions options)
    {
        // Check to see if it's a string first.
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string");

        var dateString = reader.GetString();
        // If the parse fails, then it's not in the right format,
        // and we should throw an exception anyway.
        var date = DateTime.Parse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

        return new MaxDateKeyword(date);
    }

    public override void Write(Utf8JsonWriter writer,
                               MaxDateKeyword value,
                               JsonSerializerOptions options)
    {
        writer.WritePropertyName(MaxDateKeyword.Name);
        writer.WriteStringValue(value.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
    }
}
```

## Registering the Keyword

Now that we have the keyword, we need to tell the system about it.

```c#
SchemaKeywordRegistry.Register<MaxDateKeyword>();
```

***NOTE** If you're building a dynamic system where you don't always want the keyword supported, it can be removed using the `SchemaKeywordRegistry.Unregister<T>()` static method.*

## Defining a Vocabulary

Vocabularies are used within JSON Schema to ensure that the validator you're using supports your new keyword.  Because we have already created the keyword and registered it, we know it is supported.

However, we might not be implementing _our_ vocabulary.  This keyword is likely from a third party who has written a schema that declares a vocabulary that defines `maxDate`, and we're trying to support _that_.

In accordance with the specification, JsonSchema<nsp>.Net will refuse to process any schema whose meta-schema declares a vocabulary it doesn't know about.  Because of this, it won't process the third-party schema unless we define the vocabulary on our end.

```c#
static class ThirdPartyVocabularies
{
    // Define the vocabulary and list the keyword types it defines.
    public static readonly Vocabulary DatesVocabulary =
        new Vocabulary("http://mydates.com/vocabulary", typeof(MaxDateKeyword));

    // Although not required a vocabulary may also define a meta-schema.
    // It's a good idea to implement that as well.
    public static readonly JsonSchema DatesMetaSchema =
        new JsonSchemaBuilder()
            .Id("http://mydates.com/schema")
            .Schema(MetaSchemas.Draft201909Id)
            .Vocabulary(
                (Vocabularies.Core201909Id, true),
                ("http://mydates.com/vocabulary", true)
            )
            .Properties(
                (MaxDateKeyword.Name, new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Format(Formats.DateTime)
                )
            );
}
```

Then they need to be registered.  This is done on the schema validation options.

```c#
options.SchemaRegistry.Register(new Uri("http://mydates.com/schema"), DatesMetaSchema);
options.VocabularyRegistry.Register(DatesVocabulary);
```