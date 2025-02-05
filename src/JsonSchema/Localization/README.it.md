## Riepilogo

_JsonSchema.Net.it_ estende [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) per fornire traduzioni dei messaggi di errore in italiano.

## Collegamenti

- [Documentazione](https://docs.json-everything.net/pointer/basics/)
- [Riferimento API](https://docs.json-everything.net/api/JsonPointer.Net/JsonPointer/)
- [Note sulla versione](https://docs.json-everything.net/rn-json-pointer/)

## Utilizzo

Imposta la cultura a livello globale:

```c#
ErrorMessages.Culture = CultureInfo.GetCultureInfo("it");
```

o nelle opzioni:

```c#
var options = new EvaluationOptions
{
      Culture = CultureInfo.GetCultureInfo("it")
}
```