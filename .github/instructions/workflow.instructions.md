---
applyTo: "**"
---

# Agent Workflow

This file defines how AI agents MUST orient themselves before making changes in this repository.

Each step in this section MUST be executed as an explicit tool call. A step MUST NOT be skipped; having a file's content already in context is not a valid reason to omit the tool call.

## Before Making Any Code Changes

1. List instruction files in `.github/instructions/*.instructions.md` and read relevant files for the task.
2. The following files MUST always be read via explicit tool calls before any other action:
   - `workflow.instructions.md`
   - `commands.instructions.md`
   - `personal-preferences.instructions.md`
3. Read additional files by concern:
   - `code-style.instructions.md` when editing C# code.
   - `architecture.instructions.md` when adding projects, files, package references, or changing structure.
   - `tests.instructions.md` when editing tests or changing behavior.
   - `apis.instructions.md` when editing `src/JsonSchema.Api/` or its tests.
   - `domain-knowledge.instructions.md` for JSON-domain behavior tied to specifications and package intent.
   - `domain-analyzers.instructions.md` for analyzer/source-generator diagnostics, release tracking files, or analyzer packaging behavior.
   - `instructions.instructions.md` when editing `.instructions.md` files.
4. For C# style and formatting, treat `.editorconfig` and `json-everything.sln.DotSettings` in the repo root as authoritative.
5. Prefer minimal, targeted changes. Do not refactor unrelated code.

## Principles

- Treat instruction files as the desired state for future contributions.
- Preserve public API compatibility unless the task explicitly requires a breaking change.
- Keep changes aligned with the repository's library-first design (packages under `src/`).
- Add or update tests for behavioral changes.
- Keep instructions dynamic. Do not wait for an explicit request to update instruction files when feedback reveals a missing or incorrect rule.
- If the user corrects a mistake or asks for a specific behavior — even once — update the relevant instruction file immediately alongside the code fix, without waiting to be asked.
- If an attempted fix does not work, revert that failed change before trying a different solution.
