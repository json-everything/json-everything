---
applyTo: "tests/**/*.cs,tests/**/*.json,tests/**/*.yaml,tests/**/*.txt"
---

# Testing Instructions

This repository uses NUnit-based test projects for package validation.

## Framework And Style

- Use NUnit attributes (`[Test]`, `[TestCase]`, `[TestCaseSource]`, etc.).
- Prefer `Assert.That(...)` style assertions for consistency.
- Prefer NUnit assertions over other assertion frameworks.
- Do not use FluentAssertions or any other "natural language" assertion library. FluentAssertions has been removed from the repository.
- Reuse helpers from `tests/TestHelpers/` where available.

## Test Placement

- Place tests in the corresponding `tests/<PackageName>.Tests/` project.
- Keep test file and class names clear and behavior-oriented.
- For regression fixes, add or update a test that fails before the fix and passes after.
- For pending functionality, do not mark new tests with `[Ignore]` or equivalent skip attributes; keep them failing to preserve TDD red-green flow unless the user explicitly requests otherwise.

## Data-Driven And Spec Tests

- Many tests validate against JSON/spec fixture files. Keep fixtures close to related tests.
- Prefer extending existing fixture-driven tests over creating ad-hoc one-off harnesses.
- Preserve fixture readability and stable ordering to keep diffs understandable.

## Runtime And Environment Notes

- CI runs tests across modern .NET versions.
- Some test projects include Windows-only targets (such as `net481`), with conditional behavior on non-Windows systems.

## Local Execution

- Run focused tests first for touched projects, then broader suites as needed.
- If additional test logging is needed, set `JSON_EVERYTHING_TEST_OUTPUT=True`.
