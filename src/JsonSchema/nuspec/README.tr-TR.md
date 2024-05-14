## Özet

_JsonSchema.Net.tr-TR_, hata mesajlarının Türkçeye çevirisini sağlamak için [_JsonSchema.Net_](https://www.nuget.org/packages/JsonSchema.Net) hizmetini genişletiyor.

## Bağlantılar

- [Belgeler](https://docs.json-everything.net/pointer/basics/)
- [API Referansı](https://docs.json-everything.net/api/JsonPointer.Net/JsonPointer/)
- [Sürüm Notları](https://docs.json-everything.net/rn-json-pointer/)

## Kullanmak

Kültürü küresel olarak ayarlayın:

```c#
ErrorMessages.Culture = CultureInfo.GetCultureInfo("tr-TR");
''''

veya seçeneklerde:

```c#
var options = new EvaluationOptions
{
      Culture = CultureInfo.GetCultureInfo("tr-TR")
}
''''