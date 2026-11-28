---
applyTo: "*.instructions.md"
---

# Instructions For Instruction Files

This document defines standards for authoring instruction files in this repository.

## File Location And Naming

- Location: `.github/instructions/`
- Extension: `.instructions.md`
- Naming: kebab-case (example: `code-style.instructions.md`)

## Frontmatter

Every instruction file must start with YAML frontmatter.

Required shape:

```yaml
---
applyTo: "*.cs"
---
```

Rules:

- `applyTo` is a single string.
- Use comma-delimited globs when multiple patterns are needed.
- Keep patterns scoped; avoid `"**"` unless the instruction truly applies everywhere.

## Content Rules

- Write the desired state, not legacy behavior.
- Keep each file focused on one concern.
- Prefer short, concrete rules.
- Include examples only when they clarify a non-obvious rule.
- Avoid copying generic enterprise API guidance that does not match this repository.

## Maintenance

- Update instruction files when conventions evolve.
- Remove outdated guidance promptly.
- Keep overlap low across files; if a rule belongs in one file, reference it from others instead of repeating it.
- Treat instruction files as living guidance. Apply proactive updates when recurring user requests or repeated corrections indicate a stable preference or workflow rule.
- Do not wait for a direct "update instructions" request when a clear pattern is present.
- Capture repeated mistakes as explicit rules so the same error is less likely to recur.
