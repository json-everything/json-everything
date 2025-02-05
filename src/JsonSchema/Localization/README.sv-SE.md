## Sammanfattning

_JsonSchema.Net.sv-SE_ utökar [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) för att tillhandahålla översättningar av felmeddelanden till svenska.

## Länkar

- [Dokumentation](https://docs.json-everything.net/pointer/basics/)
- [API-referens](https://docs.json-everything.net/api/JsonPointer.Net/JsonPointer/)
- [Release Notes](https://docs.json-everything.net/rn-json-pointer/)

## Använda sig av

Ställ in kulturen globalt:

```c#
ErrorMessages.Culture = CultureInfo.GetCultureInfo("sv-SE");
```

eller i alternativen:

```c#
var options = new EvaluationOptions
{
      Culture = CultureInfo.GetCultureInfo("sv-SE")
}
```