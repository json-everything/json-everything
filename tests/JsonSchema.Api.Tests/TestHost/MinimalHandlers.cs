using Microsoft.AspNetCore.Http;

namespace Json.Schema.Api.Tests.TestHost;

/// <summary>
/// Handlers mapped as method groups rather than lambdas, so the body the generator reads is a
/// separate declaration from the `Map*` call.
/// </summary>
public static class MinimalHandlers
{
	/// <summary>
	/// Echoes the model.
	/// </summary>
	/// <param name="model">The model to echo.</param>
	/// <returns>The same model.</returns>
	[Tags("MethodGroup")]
	public static IResult Handle(SimpleModel model)
	{
		return Results.Ok(model);
	}
}
