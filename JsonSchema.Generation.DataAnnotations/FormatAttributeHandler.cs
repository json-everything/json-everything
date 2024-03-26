using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.DataAnnotations;

public abstract class FormatAttributeHandler<T> : IAttributeHandler<T>
	where T : Attribute
{
	public Format Format { get; }

	public FormatAttributeHandler(string format)
	{
		Format = new Format(format);
	}

	public FormatAttributeHandler(Format format)
	{
		Format = format;
	}

	public virtual void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Add(new FormatIntent(Format));
	}
}
