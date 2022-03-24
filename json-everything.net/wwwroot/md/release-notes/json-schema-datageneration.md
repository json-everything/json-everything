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
