## Streszczenie

_JsonSchema.Net.pl-PL_ rozszerza [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) o tłumaczenie komunikatów o błędach na język polski.

## Linki

- [Dokumentacja](https://docs.json-everything.net/pointer/basics/)
- [Dokumentacja API](https://docs.json-everything.net/api/JsonPointer.Net/JsonPointer/)
— [Uwagi do wersji](https://docs.json-everything.net/rn-json-pointer/)

## Używać

Ustaw kulturę globalnie:

```c#
ErrorMessages.Culture = CultureInfo.GetCultureInfo("pl-PL");
```

lub w opcjach:

```c#
var options = new EvaluationOptions
{
    Culture = CultureInfo.GetCultureInfo("pl-PL")
}
```