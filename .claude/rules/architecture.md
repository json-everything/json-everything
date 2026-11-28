---
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/Directory.Build.props"
  - "*.slnx"
---

# Architecture Instructions

- Add new functionality to the most specific existing package. Add a new package only when the behavior does not fit an existing one.
- Do not remove target frameworks unless explicitly requested.
- Preserve signing, package metadata and release-note links.
- Settings shared by every project in a folder go in `src/Directory.Build.props` or `tests/Directory.Build.props`. A project file holds only what is specific to that project or overrides a shared default.
- Do not add a root-level props file. Settings common to both folders go in both files.
