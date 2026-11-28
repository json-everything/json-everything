# CLAUDE.md

Rules that apply to every task are in this file. Rules that apply to particular files are in `.claude/rules/`, and each one lists the files it covers in its `paths` frontmatter.

## Principles

- Treat this file and the rule files as the desired state for future contributions.
- Prefer minimal, targeted changes. Do not refactor unrelated code.
- Preserve public API compatibility unless the task explicitly requires a breaking change.
- Keep changes aligned with the repository's library-first design (packages under `src/`).
- Add or update tests for behavioral changes.
- Follow TDD principles for bug fixes. Add and run replicating tests first to verify the bug, then apply the fix.
- If an attempted fix does not work, revert that failed change before trying a different solution.
- XML documentation, release notes and these instruction files state the exact condition or number a rule depends on, in place of vague qualifiers such as "rarely" or "where possible".

## Commands

Set `JSON_EVERYTHING_TEST_OUTPUT` to `True` for diagnostic output from tests:

```bash
# PowerShell
$env:JSON_EVERYTHING_TEST_OUTPUT = "True"
dotnet test
```

Pack a package with `ResourceLanguage=base`:

```bash
dotnet build src/JsonSchema/JsonSchema.csproj -c Release --no-restore
dotnet pack src/JsonSchema/JsonSchema.csproj -o src/JsonSchema/nupkg -c Release --no-build -p:ResourceLanguage=base
```

Generate the documentation into an output folder:

```bash
dotnet build tools/ApiDocsGenerator/ApiDocsGenerator.csproj -c Release
dotnet tools/ApiDocsGenerator/bin/Release/net10.0/ApiDocsGenerator.dll doc-output-path
```
