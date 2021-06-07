# Serializing Enumerations by Name

By default, the `System.Text.Json` serializer will convert enumeration members to their numeric values.  But suppose we're interacting with an API that expects named values.  To do this, we need to tell the serializer how to convert the enum values into strings.  This is the purpose of the `EnumStringConverter<T>` class.

Let's assume that the API we're trying to talk with has a `MemberType` enumeration with the values `free-tier`, `pro`, and `enterprise`.

We can model this in our code with

```c#
enum MemberType
{
    Default, // we don't want to default to a valid value
    FreeTier,
    Pro,
    Enterprise
}
```

but as mentioned, this just renders as numbers in the JSON.  Let's add the converter.

```c#
[JsonConverter(typeof(EnumStringConverter<MemberType>))]
enum MemberType
{
    Default, // we don't want to default to a valid value
    FreeTier,
    Pro,
    Enterprise
}
```

This will now render the values as they appear in the C# code, in _PascalCase_.  Still not quite what the API needs.  So let's add some `DescriptionAttribute`s from the `System.ComponentModel` namespace.

```c#
[JsonConverter(typeof(EnumStringConverter<MemberType>))]
enum MemberType
{
    Default, // we don't want to default to a valid value
    [Description("free-tier")]
    FreeTier,
    [Description("pro")]
    Pro,
    [Description("enterprise")]
    Enterprise
}
```

Now the converter will use these strings for the corresponding vauues.

## Flag Enums

When an enum has the `[Flags]` attribute, the converter will output the flag values as a comma-delimited list of string values instead of just a single value.  This covers the case where multiple values are "selected."

```c#
[Flags]
[JsonConverter(typeof(EnumStringConverter<SnowConeColors>))]
enum SnowConeColors
{
    [Description("none")]
    None = 0,
    [Description("red")]
    Red = 1,
    [Description("orange")]
    Orange = 2,
    [Description("yellow")]
    Yellow = 4,
    [Description("green")]
    Green = 8,
    [Description("blue")]
    Blue = 16,
    [Description("purple")]
    Purple = 32
}
```

In this case, when we want to serialize the enum value `Orange | Green`, we'll get `"green,orange"` in the serialization.