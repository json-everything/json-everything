using Json.Schema.Generation;
using Json.Schema.Generation.Serialization;

namespace Json.Schema.Api.Tests.TestHost;

[GenerateJsonSchema]
public record SimpleModel(
	[property: Required]
	string Name,
	int Age);

[GenerateJsonSchema]
[AdditionalProperties(false)]
public record StrictModel(
	[property: Required]
	string Title,
	bool Active);

[GenerateJsonSchema]
[AdditionalProperties(false)]
public record MultiWordModel(
	[property: Required]
	string FirstName,
	[property: Required]
	string LastName);

public record UnvalidatedModel(
	string Description);
