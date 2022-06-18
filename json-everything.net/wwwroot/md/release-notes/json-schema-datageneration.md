# 1.0.1 (no PR)

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.

# [1.0.0](https://github.com/gregsdennis/json-everything/pull/280)

Updated all functionality to use `JsonNode` instead of `JsonElement` in line with JsonSchema.Net v3.

Also... OFFICIAL RELEASE!!!  I feel confident enough in this library to release it under a real version!  Thought, the JSON Schema team still advises that this sort of functionality should be used as a development tool, not in production scenarios.

# [0.1.3](https://github.com/gregsdennis/json-everything/pull/262)

Improved handling of conditionals and inferred types.

# [0.1.2](https://github.com/gregsdennis/json-everything/pull/259)

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

Better support for const, specifically when it appears in a conditional (`if`/`then`/`else`) or under a `not`.

# 0.1.1 (No PR)

Fixed a bug around property generation.

# [0.1.0](https://github.com/gregsdennis/json-everything/pull/218)

Initial release.

Not supported:

- anything involving RegEx
- reference keywords (e.g. `$ref`, `$dynamicRef`, etc)
- annotation / metadata keywords (e.g. `title`, `description`)
- `content*` keywords
- `dependencies` / `dependent*` keywords

Instance generation isn't 100%, but works most of the time.
