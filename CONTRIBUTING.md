# Local Setup

## Requirements

These libraries run tests in .Net Core 3.1 and .Net 5, so you'll need those.

You'll also need .Net 6 installed to support the auto-formatting hook below.

There are definitely some C#8 features in the code.  All of the projects are configured to use the latest C# version.

## IDE

I use Visual Studio Community with Resharper, and I try to keep everything updated.

Jetbrains Rider (comes with the Resharper stuff built-in), VS Code with your favorite extensions, or any basic text editor with a command line would work just fine.  You do you.

## Code Style

Whatever code editor you use, please add the following pre-commit git hook:

```sh
#!/bin/sh

LC_ALL=C
# Select files to format
FILES=$(git diff --cached --name-only --diff-filter=ACM "*.cs" | sed 's| |\\ |g')
[ -z "$FILES" ] && exit 0

# Format all selected files
echo "$FILES" | cat | xargs | sed -e 's/ /,/g' | xargs dotnet format json-everything.sln --include

# Add back the modified files to staging
echo "$FILES" | xargs git add

exit 0
```

This will run `dotnet format` on any changed files.

I've added a build step that posts to a PR if the code isn't formatted correctly.  If you add the hook above, you _shouldn't_ have any problems, but in my experience, there are a few things that the check catches that the hook (which is intended to auto-fix issues) doesn't.

# What Needs Doing?

Anything in the [issues](https://github.com/gregsdennis/json-everything/issues?q=is%3Aopen+is%3Aissue+label%3A%22help+wanted%22) with a `help wanted` label is something that could benefit from a volunteer or two.

Of primary focus is translating the [resource file](https://github.com/gregsdennis/json-everything/blob/master/JsonSchema/Localization/Resources.resx) into additional languages for JsonSchema.Net.

Outside of this, PRs are welcome.  For larger changes or changes to the API surface, it's preferred that there be some discussion in an issue before a PR is submitted, just to discuss the specifics of the change.  Mainly, you shouldn't feel like you've wasted your time if changes are requested or the PR is ultimately closed unmerged.