using System;

namespace Json.Schema.Generation;

internal class AdHocGenerationContext : SchemaGenerationContextBase
{
	public override Type Type => null!;

	internal override void GenerateIntents()
	{
		throw new NotImplementedException();
	}
}