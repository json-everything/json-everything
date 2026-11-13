---
layout: page
title: JsonSchema.Net.Generation.DataAnnotations
icon: fas fa-tag
order: "09.06"
---
# [3.2.0](https://github.com/json-everything/json-everything/pull/1077) {#release-schemagendataannotations-3.2.0}

Add .Net 11.0 support.

[#1076](https://github.com/json-everything/json-everything/issues/1076) - .Net 11 changes how `double` is converted to `decimal`.  When running under .Net 11, `[Range]` attributes with `double` bounds produce the exact binary value for `minimum` and `maximum` (e.g. `4.7889999999999997015720509808` instead of `4.789`).

This is only a minor version bump because this package does not itself introduce a break: updating to this version without updating to .Net 11 changes nothing, and updating to .Net 11 without updating to this version results in the same change in behavior.  It is the .Net update that changes behavior, not this package.

Removed the unnecessary `System.ComponentModel.Annotations` package dependency for .Net 10.0.  It is now only a dependency for .Net Standard 2.0.

# [3.1.1](https://github.com/json-everything/json-everything/pull/1013) {#release-schemagendataannotations-3.1.1}

Updated nuget packages & EULA.

# [3.1.0](https://github.com/json-everything/json-everything/pull/991) {#release-schemagendataannotations-3.1.0}

[#986](https://github.com/json-everything/json-everything/issues/986) - Adds source generation support with _JsonSchema.Net.Generation_ v7.1.0.  Thanks to [@ThomasNieto](https://github.com/ThomasNieto) for suggesting.

# [3.0.0](https://github.com/json-everything/json-everything/pull/982) {#release-schemagendataannotations-3.0.0}

Enacted Open Source Maintenance Fee.

See the [blog post](https://blog.json-everything.net/posts/expensive/) for more information.

# [2.0.0](https://github.com/json-everything/json-everything/pull/952) {#release-schemagendataannotations-2.0.0}

Updated per _JsonSchema.Net_ v8 (breaking changes).

- Added `Dialect` static class.
- Add .Net 10.0 support.
- Update Nuget references.

# [1.2.0](https://github.com/json-everything/json-everything/pull/822) {#release-schemagendataannotations-1.2.0}

Add .Net 9.0 support.

# [1.1.0](https://github.com/json-everything/json-everything/pull/719) {#release-schemagendataannotations-1.1.0}

Updated to use _JsonSchema.Net_ v7.0.0, which contains breaking changes ([release notes](/rn-json-schema/#release-schema-7.0.0)).

# [1.0.0.x](https://github.com/json-everything/json-everything/pull/712) {#release-schemagendataannotations-1.0.0.x}

[#711](https://github.com/json-everything/json-everything/issues/711) - Nuget package meta-data updates; no functional changes from previous version.

# [1.0.0](https://github.com/json-everything/json-everything/pull/41) {#release-schemagendataannotations-1.0.0}

Initial release.
