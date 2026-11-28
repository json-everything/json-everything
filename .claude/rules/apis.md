---
paths:
  - "src/JsonSchema.Api/**/*.cs"
  - "tests/JsonSchema.Api.Tests/**/*.cs"
---

# API Instructions

- Preserve the default validation behavior unless a change is explicitly requested.
- Prefer additive options over breaking option changes.
- Changes must work for both MVC controllers and minimal APIs.
- Validation failures must keep producing stable problem details payloads, with JSON Pointer error keys where model state is translated.
- Handle both partial and full binding failures.
- When changing validation paths, tests verify the status code, content type and error payload shape.
