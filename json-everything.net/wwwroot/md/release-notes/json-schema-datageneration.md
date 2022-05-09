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
