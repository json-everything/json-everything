---
paths:
  - "tests/**/*.cs"
  - "tests/**/*.json"
  - "tests/**/*.yaml"
  - "tests/**/*.txt"
---

# Testing Instructions

- Use `Assert.That(...)` assertions.
- Do not use FluentAssertions or any other natural-language assertion library.
- Reuse helpers from `tests/TestHelpers/` where they fit.
- Do not widen the visibility of implementation details for tests. Cover them through assertions on the public behaviour that uses them.
- Tests carry no comments recording history, such as an issue number, a defect description or a fix date. The test name states the scenario and outcome.
- Do not mark tests for pending functionality with `[Ignore]` or another skip attribute. Leave them failing unless the user explicitly asks otherwise.
- Extend existing fixture-driven tests instead of writing one-off harnesses.
- Keep fixture files readable and their ordering stable, so diffs stay small.
- Run focused tests for the touched projects before broader suites.
