---
applyTo: "tools/ApiDocsGenerator/release-notes/*.md"
---

# Release Notes Instructions

Use these rules when updating package release notes in `tools/ApiDocsGenerator/release-notes/`.

## Entry Placement And Structure

- Add new releases at the top of the file, immediately after frontmatter.
- Use the existing heading format: `# [x.y.z](link) {#anchor}`.
- Keep release body concise and focused on user-visible behavior.

## Link And Reference Rules

- If the heading link points to a PR, do not repeat that PR link in the body text.
- Prefer issue links in body bullets when available.
- Reference issue numbers that establish bug context.

## Attribution Rules

- If the issue reporter also authored the fix (determine from commit author), thank them for the fix instead of reporting.
- Use: `Thanks to [@handle](https://github.com/handle) for the fix.` when they submitted the fix.
- Otherwise, credit issue reporters when known using: `Thanks to [@handle](https://github.com/handle) for reporting.`
- Place attribution in the same paragraph as the related issue note.

## Content Rules

- Describe impact and outcome, not internal implementation details.
- Keep language factual; avoid speculative statements.
- Match existing tone and markdown style in the target release-notes file.