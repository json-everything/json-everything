---
paths:
  - "src/**/*.cs"
---

# Domain Knowledge Instructions

- Follow the specifications. Correctness and interoperability come before convenience shortcuts.
- Do not duplicate functionality that other ecosystems already serve well unless it adds clear value to this suite.
- Preserve support for every draft or specification version a package already implements.
- When a specification is ambiguous, choose the interpretation most consistent with it and add tests that document the choice.
- Avoid unnecessary allocations in parsing and evaluation paths.
- Keep serialization output deterministic.
- When a behavior change affects users, update the package's release notes.
