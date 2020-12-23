[![Build status](https://github.com/gregsdennis/json-everything/workflows/.NET%20Core/badge.svg)](https://github.com/gregsdennis/json-everything/actions?query=workflow%3A%22.NET+Core%22)
![Test results](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/gregsdennis/28607f2d276032f4d9a7f2c807e44df7/raw/test-results-badge.json)
[![Percentage of issues still open](http://isitmaintained.com/badge/open/gregsdennis/json-everything.svg)](http://isitmaintained.com/project/gregsdennis/json-everything "Percentage of issues still open")
[![Average time to resolve an issue](http://isitmaintained.com/badge/resolution/gregsdennis/json-everything.svg)](http://isitmaintained.com/project/gregsdennis/json-everything "Average time to resolve an issue")
![License](https://img.shields.io/github/license/gregsdennis/json-everything)
<!-- ![StackOverflow questions](https://img.shields.io/stackexchange/stackoverflow/t/json-everything) -->

|JsonSchema.Net|JsonSchema.Net<br>.Generation|JsonPointer.Net|JsonPath.Net|
|:-:|:-:|:-:|:-:|
|<a href="https://www.nuget.org/packages/JsonSchema.Net/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/JsonSchema.Net.svg?svg=true"></img><br><img alt="NuGet version" src="https://img.shields.io/nuget/dt/JsonSchema.Net.svg?svg=true"></img></a>|<a href="https://www.nuget.org/packages/JsonSchema.Net.Generation/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/JsonSchema.Net.Generation.svg?svg=true"></img><br><img alt="NuGet version" src="https://img.shields.io/nuget/dt/JsonSchema.Net.Generation.svg?svg=true"></img></a>|<a href="https://www.nuget.org/packages/JsonPointer.Net/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/JsonPointer.Net.svg?svg=true"></img><br><img alt="NuGet version" src="https://img.shields.io/nuget/dt/JsonPointer.Net.svg?svg=true"></img></a>|<a href="https://www.nuget.org/packages/JsonPath.Net/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/JsonPath.Net.svg?svg=true"></img><br><img alt="NuGet version" src="https://img.shields.io/nuget/dt/JsonPath.Net.svg?svg=true"></img></a>|

|JsonPatch.Net|JsonLogic.Net|Json.More.Net|
|:-:|:-:|:-:|
|<a href="https://www.nuget.org/packages/JsonPatch.Net/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/JsonPatch.Net.svg?svg=true"></img><br><img alt="NuGet version" src="https://img.shields.io/nuget/dt/JsonPatch.Net.svg?svg=true"></img></a>|<a href="https://www.nuget.org/packages/JsonLogic/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/JsonLogic.svg?svg=true"></img><br><img alt="NuGet version" src="https://img.shields.io/nuget/dt/JsonLogic.svg?svg=true"></img></a>|<a href="https://www.nuget.org/packages/Json.More.Net/"><img alt="NuGet version" src="https://img.shields.io/nuget/v/Json.More.Net.svg?svg=true"></img><br><img alt="NuGet version" src="https://img.shields.io/nuget/dt/Json.More.Net.svg?svg=true"></img></a>|

<a href="https://join.slack.com/t/manateeopensource/shared_invite/enQtMzU4MjgzMjgyNzU3LWZjYzAzYzY3NjY1MjY3ODI0ZGJiZjc3Nzk1MDM5NTNlMjMyOTE0MzMxYWVjMjdiOGU1NDY5OGVhMGQ5YzY4Zjg"><img src="Resources/Slack.png" alt="Discuss on Slack" title="Discuss on Slack" height="75"></a>
<a href="http://www.jetbrains.com/resharper"><img src="Resources/Resharper.svg" alt="Made with Jetbrains Resharper" title="Made with Jetbrains Resharper" height="75"></a>

[Documentation](https://gregsdennis.github.io/json-everything)

`json-everything` is your one-stop shop for extending the JSON functionality provided by .Net Core 3's `System.Text.Json` namespace, all (well, most anyway) provided in convenient .Net Standard 2.0 packages.

Currently supported:

- [JSON Pointer (RFC 6901)](https://tools.ietf.org/html/rfc6901)
- [Relative JSON Pointer](https://tools.ietf.org/id/draft-handrews-relative-json-pointer-00.html)
- [JSON Schema (drafts 6 and higher)](https://json-schema.org)
- [JSON Path (RFC in progress)](https://github.com/jsonpath-standard/internet-draft) (.Net Standard 2.1)
- [JSON Patch (RFC 6902)](https://tools.ietf.org/html/rfc6902)
- [JsonLogic](https://jsonlogic.com) (.Net Standard 2.1)
