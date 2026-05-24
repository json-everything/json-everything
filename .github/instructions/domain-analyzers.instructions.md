---
applyTo: "*.cs,*.csproj,src/**/AnalyzerReleases.Shipped.md,src/**/AnalyzerReleases.Unshipped.md"
---

# Domain Knowledge: Analyzers

This file defines analyzer and source-generator domain rules for this repository.

## Release Tracking

- Record new diagnostic IDs directly in `AnalyzerReleases.Shipped.md`.
- Keep `AnalyzerReleases.Unshipped.md` empty unless explicitly requested otherwise.
- Keep shipped entries concise and stable: Rule ID, Category, Severity, and short notes.

## Analyzer Contracts

- Treat `DiagnosticDescriptor` metadata (ID, category, default severity, title/message intent) as a public contract.
- Keep diagnostic IDs stable once introduced; do not reuse IDs for different intent.
- When changing analyzer behavior in a user-visible way, update rule notes and tests where applicable.

## Project Wiring

- Analyzer projects should include release tracking files as `AdditionalFiles` so release-tracking analyzers can consume them.
- Keep analyzer packaging and `IsRoslynComponent` behavior aligned with existing project patterns.
