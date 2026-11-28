---
paths:
  - "CLAUDE.md"
  - "AGENTS.md"
  - ".github/copilot-instructions.md"
  - ".claude/rules/**/*.md"
---

# Instructions For Agent Instruction Files

## File Layout

- `CLAUDE.md` holds rules that apply to every task.
- `.claude/rules/` holds rules that apply to particular files, one concern per file.
- `AGENTS.md` and `.github/copilot-instructions.md` only point other agents at `CLAUDE.md` and `.claude/rules/`. Do not put rules in them.
- `CLAUDE.local.md` holds a contributor's personal preferences and is not committed.

## Rule Files

- Every rule file starts with YAML frontmatter holding a `paths` list of globs, matched from the repository root:

  ```yaml
  ---
  paths:
    - "src/**/*.cs"
  ---
  ```

- A rule that applies to every task goes in `CLAUDE.md`, not in a rule file without `paths`.

## Content

- These files are instructions, not documentation. Do not include anything an agent can work out from the code, such as project structure, what a package does, or conventions the code already follows consistently.
- Write the desired state, not legacy behavior.
- Prefer short, concrete rules.
- Include examples only when they clarify a non-obvious rule.
- Keep a rule in one file only.
- Remove outdated rules promptly.
