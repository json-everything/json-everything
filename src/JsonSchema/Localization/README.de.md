## Zusammenfassung

_JsonSchema.Net_ implementiert die volle [JSON Schema](https://json-schema.org/) Spezifikation, eine deklarative Syntax zur Validierung und Annotation von JSON-Daten.

Unterstützte Spezifikationen:

- Draft 6 - `http://json-schema.org/draft-06/schema#`
- Draft 7 - `http://json-schema.org/draft-07/schema#`
- Draft 2019-09 - `https://json-schema.org/draft/2019-09/schema`
- Draft 2020-12 - `https://json-schema.org/draft/2020-12/schema`

Dieses Projekt dient auch als Testumfeld für Funktionen, die für die nächste Version vorgeschlagen wurden ("draft/next").

## Links

- [Dokumentation](https://docs.json-everything.net/schema/basics/)
- [API Referenz](https://docs.json-everything.net/api/JsonSchema.Net/JsonSchema/)
- [Versionshinweise](https://docs.json-everything.net/rn-json-schema/)

## Verwendung

Globales setzen der Kultur:

```c#
ErrorMessages.Culture = CultureInfo.GetCultureInfo("de");
```

oder in den Optionen:

```c#
var options = new EvaluationOptions
{
    Culture = CultureInfo.GetCultureInfo("de")
}
```