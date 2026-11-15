---
applyTo: "**"
---

# Essential Commands

## Restore And Build

```bash
dotnet restore
dotnet build --configuration Release --no-restore
```

## Run Tests

```bash
dotnet test --no-restore --verbosity normal --logger:"trx;LogFileName=test-results.trx"
```

Focused examples:

```bash
dotnet test tests/JsonSchema.Tests/JsonSchema.Tests.csproj
dotnet test tests/JsonSchema.Api.Tests/JsonSchema.Api.Tests.csproj
```

Optional local diagnostic output:

```bash
# PowerShell
$env:JSON_EVERYTHING_TEST_OUTPUT = "True"
dotnet test
```

## Package Build/Pack (Example)

```bash
dotnet build src/JsonSchema/JsonSchema.csproj -c Release --no-restore
dotnet pack src/JsonSchema/JsonSchema.csproj -o src/JsonSchema/nupkg -c Release --no-build -p:ResourceLanguage=base
```

## Submodules

```bash
git submodule update --init
```

## Documentation Tooling

```bash
dotnet build tools/ApiDocsGenerator/ApiDocsGenerator.csproj -c Release
dotnet tools/ApiDocsGenerator/bin/Release/net10.0/ApiDocsGenerator.dll doc-output-path
```
