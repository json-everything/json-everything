---
layout: page
title: JsonSchema.Net
icon: fas fa-tag
order: "09.055"
---
# [1.2.2](https://github.com/json-everything/json-everything/pull/1061) {#release-schemaapi-1.2.2}

Fixed analyzer `EnumFormat` output.

# [1.2.1](https://github.com/json-everything/json-everything/pull/1061) {#release-schemaapi-1.2.1}

- Fixed a generator packing issue.
- Route prefixes from `MapGroup` are now kept when fluent calls such as `.WithTags()` are chained onto the group.
- Minimal-API handler parameters honor `[FromBody]`, `[FromRoute]`, `[FromQuery]`, `[FromHeader]`, and `[FromServices]`.  Injected services, including interfaces and abstract types, are no longer described as request bodies or parameters.
- Nullable query and header parameters are described as optional.
- Enum parameters are described inline.
- Validation registers the enum converter matching the project's `JsonSchemaDefaultEnumFormat` (see _JsonSchema.Net.Generation_ 7.4.0) when no enum converter is present, so the serializer accepts what the generated schemas describe.

# [1.2.0](https://github.com/json-everything/json-everything/pull/1060) {#release-schemaapi-1.2.0}

Added OpenAPI 3.1 support via `.AddOpenApi()`.

- Describes controllers and minimal APIs at compile time, including endpoints declared in referenced assemblies.
- Serves the description as JSON or YAML, and optionally writes it to disk.
- Serves an interactive reference page.
- Documents the `application/problem+json` response returned for validation failures.
- Registers request validation by default.  Set `AddValidation` to false to opt out.

# [1.1.1](https://github.com/json-everything/json-everything/pull/1023) {#release-schemaapi-1.1.1}

Add debug logging for full validation results on failure.

# [1.1.0](https://github.com/json-everything/json-everything/pull/1013) {#release-schemaapi-1.1.0}

Added Minimal API support.

# [1.0.3](https://github.com/json-everything/json-everything/pull/1013) {#release-schemaapi-1.0.3}

Updated nuget packages & EULA.

# [1.0.1 & 1.0.2](https://github.com/json-everything/json-everything/pull/991) {#release-schemaapi-1.0.1}

Updated to take advantage of _JsonSchema.Net.Generation_ v7.1.0 which includes source generation.

# [1.0.0](https://github.com/json-everything/json-everything/pull/989) {#release-schemaapi-1.0.0}

Initial release.
