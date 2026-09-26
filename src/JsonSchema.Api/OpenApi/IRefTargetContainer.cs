using System;

namespace Json.Schema.Api.OpenApi;

internal interface IRefTargetContainer
{
	object? Resolve(ReadOnlySpan<string> keys);
}