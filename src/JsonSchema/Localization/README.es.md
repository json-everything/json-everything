## Summary

_JsonSchema.Net.es_ extiende [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) para proporcionar traducciones de mensajes de error al español.

## Links

- [Documentación](https://docs.json-everything.net/pointer/basics/)
- [Referencia de API](https://docs.json-everything.net/api/JsonPointer.Net/JsonPointer/)
- [Notas de lanzamiento](https://docs.json-everything.net/rn-json-pointer/)

## Uso

Establecer la cultura globalmente:

```c#
ErrorMessages.Culture = CultureInfo.GetCultureInfo("es");
```

o en las opciones:

```c#
var options = new EvaluationOptions
{
    Culture = CultureInfo.GetCultureInfo("es")
}
```