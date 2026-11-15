---
applyTo: "src/JsonSchema.Api/**/*.cs,tests/JsonSchema.Api.Tests/**/*.cs"
---

# API Instructions

These instructions apply to the `JsonSchema.Api` package and its tests.

## Scope

- This package provides ASP.NET Core integration points for JSON Schema validation.
- It supports both MVC controllers and minimal APIs.

## Integration Patterns

- Keep registration entry points in extension methods on `IServiceCollection` or related ASP.NET abstractions.
- Preserve the package's default validation behavior unless a change is explicitly requested.
- Prefer additive options over breaking option changes.

## Error Contract

- Validation failures should continue to produce stable, machine-consumable problem details payloads.
- Preserve JSON Pointer-oriented error key behavior where model-state translation is involved.

## ASP.NET Compatibility

- Avoid assumptions tied to a single hosting model; maintain support across supported ASP.NET Core patterns used by this package.
- Keep middleware/filter/model-binder interactions coherent so both partial and full binding failures are handled.

## Tests

- Add or update tests in `tests/JsonSchema.Api.Tests/` for API behavior changes.
- Verify status code, content type, and error payload shape when changing validation paths.
