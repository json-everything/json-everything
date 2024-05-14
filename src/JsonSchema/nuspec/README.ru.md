## Краткое содержание

_JsonSchema.Net.ru_ расширяет [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) для обеспечения перевода сообщений об ошибках на русский язык.

## Ссылки

- [Документация](https://docs.json-everything.net/pointer/basics/)
- [Справочник API](https://docs.json-everything.net/api/JsonPointer.Net/JsonPointer/)
- [Примечания к выпуску](https://docs.json-everything.net/rn-json-pointer/)

## Использовать

Установите культуру глобально:

```c#
ErrorMessages.Culture = CultureInfo.GetCultureInfo("ru");
```

или в опциях:

```c#
var options = new EvaluationOptions
{
      Culture = CultureInfo.GetCultureInfo("ru")
}
```