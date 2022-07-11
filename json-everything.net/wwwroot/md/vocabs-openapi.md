# OpenAPI v3.1 Vocabulary

This library adds support for the vocabularies, meta-schemas, and keywords defined by the OpenAPI v3.1 specification.

To enable this vocabulary, include the following in your startup logic to register everything.

```c#
Json.Schema.OpenApi.Vocabularies.Register();
```

Note that all of the keywords defined by this vocabulary are annotative: they will produce annotations but provide no validation logic.  The output from these keywords is intended to be used by other OpenAPI tooling.