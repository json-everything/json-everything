# AGENTS.md

This file is a mandatory entry point for AI agents working in this repository.

The repository's agent instructions are kept in the Claude Code layout. Claude Code loads them itself. Every other agent must read them as described below.

## Required Preflight (Blocking)

Before any analysis, search, planning, code edits, test execution, or other tool use, read these files using tool calls:

1. `CLAUDE.md`, which holds the rules that apply to every task.
2. `CLAUDE.local.md`, if it exists. It holds the contributor's personal preferences and is not committed.

## Rules For Particular Files

`.claude/rules/` holds rules that apply to particular files. Each rule file lists the files it covers as globs in its `paths` frontmatter, matched from the repository root.

Before reading or editing a file, read every rule file in `.claude/rules/` whose `paths` match that file.

If this preflight cannot be completed, stop and ask the user how to proceed.

Do not treat this as optional guidance.
