## 요약

JsonSchema.Net.ko는 [JsonSchema.Net](https://www.nuget.org/packages/JsonSchema.Net)을 확장하여 한국어 오류 메시지를 제공합니다.

## 링크

- [문서](https://docs.json-everything.net/pointer/basics/)
- [API 참조](https://docs.json-everything.net/api/JsonPointer.Net/JsonPointer/)
- [릴리스 노트](https://docs.json-everything.net/rn-json-pointer/)

## 사용법

문화권을 전역적으로 설정합니다.

```c#
ErrorMessages.Culture = CultureInfo.GetCultureInfo("ko");
```

또는 옵션에서:

```c#
var options = new EvaluationOptions
{
    Culture = CultureInfo.GetCultureInfo("ko")
}
```