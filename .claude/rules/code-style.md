---
paths:
  - "**/*.cs"
---

# Code Style Instructions

`.editorconfig` and `json-everything.slnx.DotSettings` are authoritative for style. Where they say nothing, follow the style of nearby files. The rules below cover what neither file enforces.

## Language Features

- Use collection expressions (`[value]`, `[a, b]`) instead of `new[] { ... }` when the target type is clear.
- Do not use digit separators in numeric literals. Write `1000000`, not `1_000_000`.
- Do not add an `Async` suffix to a method that returns `Task` or `ValueTask`. Use the suffix only when a synchronous method of the same name exists.

## Member Ordering

- Class member order:
	1. private consts and private static readonly fields
	2. private fields
	3. public consts and public static readonly fields
	4. internal consts and internal static readonly fields
	5. properties
	6. events
	7. constructors
	8. methods
- Do not use non-private mutable fields.
- For non-field members, order by access modifier: public, protected, internal, private.

## Control Flow

- Keep a single-statement `if` body on the same line as the condition: `if (condition) return;`, `if (condition) throw ...;`. This applies to `return`, `continue`, `break` and `throw`.

## Comments

- A comment explains only what the code does not show.
- A TODO states in full sentences what the code does today and what that means for its caller, then what it should do.

## JSON

- Do not introduce a JSON library other than `System.Text.Json` unless explicitly requested.
- Indent inline JSON literals in C# source, such as raw string literals, with two spaces.
