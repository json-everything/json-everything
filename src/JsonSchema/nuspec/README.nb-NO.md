## Sammendrag

_JsonSchema.Net.nb-NO_ utvider [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) for å gi oversettelser av feilmeldinger til norsk.

## Lenker

- [Dokumentasjon](https://docs.json-everything.net/pointer/basics/)
- [API-referanse](https://docs.json-everything.net/api/JsonPointer.Net/JsonPointer/)
- [Versjonsmerknader](https://docs.json-everything.net/rn-json-pointer/)

## Bruk

Sett kulturen globalt:

```c#
ErrorMessages.Culture = CultureInfo.GetCultureInfo("nb-NO");
```

eller i alternativene:

```c#
var options = new EvaluationOptions
{
      Culture = CultureInfo.GetCultureInfo("nb-NO")
}
```