---
applyTo: "*.cs"
---

# Domain Knowledge Instructions

This repository provides JSON-focused libraries built around `System.Text.Json`.

## Product Intent

- Prefer standards-aligned behavior for JSON Schema, JSON Pointer, JSON Path, JSON Patch, JsonLogic, JSON-e, and related tooling.
- Prioritize correctness and interoperability over convenience shortcuts.
- Do not duplicate functionality already well served by other ecosystems unless there is clear value for this suite.

## Library Suite Mindset

- Changes can affect many downstream consumers across multiple packages.
- Favor predictable behavior and stable defaults.
- Breaking changes require strong justification.

## Specifications And Compatibility

- Preserve support for established drafts/specs already implemented by each package.
- When behavior is ambiguous, prefer spec-consistent interpretation and add tests documenting the choice.

## Performance And Reliability

- Parsing and evaluation paths should avoid unnecessary allocations where practical.
- Keep serialization/deserialization deterministic where consumers rely on stable output.

## Documentation And Consumer Experience

- Public APIs should remain discoverable and well documented.
- If a behavior change impacts users, ensure tests and docs/release notes can reflect the change.
