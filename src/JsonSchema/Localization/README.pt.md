## Resumo

_JsonSchema.Net.pt_ estende [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) para disponibilizar traduções das mensagens de erro para português.

## Ligações

- [Documentação](https://docs.json-everything.net/pointer/basics/)
- [Referência da API](https://docs.json-everything.net/api/JsonPointer.Net/JsonPointer/)
- [Notas de lançamento](https://docs.json-everything.net/rn-json-pointer/)

## Utilização

Definir a cultura globalmente:

```csharp
ErrorMessages.Culture = CultureInfo.GetCultureInfo("pt");

ou nas opções:

```c#
var options = new EvaluationOptions
{
    Culture = CultureInfo.GetCultureInfo("pt")
}
```