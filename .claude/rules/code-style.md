---
applyTo: "*.cs"
---

# Code Style Instructions

This document covers C# style conventions that complement `.editorconfig`.

## Source Of Truth

- `.editorconfig` is authoritative for formatting, indentation, brace style, and naming rules.
- `json-everything.sln.DotSettings` (JetBrains/ReSharper) is also authoritative where it defines style or inspection behavior.
- Follow existing style in nearby files when rules are not explicit.

## Required Practices

- Use file-scoped namespaces.
- Keep nullable reference types enabled and handle nullability explicitly.
- Prefer immutable design where practical (`readonly` fields, init-only or constructor initialization).
- Use expressive type and member names; avoid abbreviations unless they are established JSON terms.
- Keep methods focused and small.

## Language Features

- Prefer modern C# language features supported by the target frameworks when they improve readability.
- Prefer collection expressions (e.g. `[value]`, `[a, b]`) instead of `new[] { ... }` when the target type is clear.

## Member Ordering

- Class member order should be:
	1. private consts and private static readonly fields
	2. private fields
	3. public consts and public static readonly fields
	4. internal consts and internal static readonly fields
	5. properties
	6. events
	7. constructors
	8. methods
- Do not use non-private mutable fields.
- For non-field members, access modifier order should be:
	1. public
	2. protected
	3. internal
	4. private

## Control Flow Style

- Keep simple single-statement `if` bodies inline on the same line.
	- Preferred: `if (condition) return;`
	- Preferred: `if (condition) return value;`
	- Preferred: `if (condition) continue;`
	- Preferred: `if (condition) break;`
	- Preferred: `if (condition) throw ...;`

## Numeric Literals

- Do not use digit separators (e.g. `1_000_000`). Write numeric literals without separators (e.g. `1000000`).

## Comments And Documentation

- Do not add comments that restate obvious code flow.
- Public package APIs commonly include XML documentation in this repo. When adding or changing public APIs, preserve existing documentation quality and style.

## JSON And Serialization Patterns

- Preserve `System.Text.Json`-centric patterns used by the repository.
- Use shared serializer contexts/options when already established by the package.
- Avoid introducing alternate JSON stacks unless explicitly requested.
- For inline JSON literals in C# source (for example raw string text blocks), use two-space indentation.

## Compatibility Mindset

- Avoid unnecessary public API churn.
- Keep binary and source compatibility in mind for package consumers.
