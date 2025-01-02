using System;
using System.Collections.Generic;

namespace Json.Schema.Generation;

/// <summary>
/// Provides base functionality and data for generation contexts.
/// </summary>
public abstract class SchemaGenerationContextBase
{
	internal class TrueType;

	internal class FalseType;

	/// <summary>
	/// Represents a true schema.
	/// </summary>
	public static readonly SchemaGenerationContextBase True = new TypeGenerationContext(typeof(TrueType));
	/// <summary>
	/// Represents a false schema.
	/// </summary>
	public static readonly SchemaGenerationContextBase False = new TypeGenerationContext(typeof(FalseType));

	/// <summary>
	/// The type.
	/// </summary>
	public abstract Type Type { get; }

	/// <summary>
	/// The keyword intents required for this type.
	/// </summary>
	public List<ISchemaKeywordIntent> Intents { get; } = [];

	internal bool IsRoot { get; init; }

#pragma warning disable CS8618
	private protected SchemaGenerationContextBase()
	{
		DebuggerDisplay = "ad-hoc";
	}
#pragma warning restore CS8618

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The schema builder.</param>
	/// <returns>The schema builder (for fluent syntax support).</returns>
	public JsonSchema Apply(JsonSchemaBuilder? builder = null)
	{
		if (ReferenceEquals(this, True)) return true;
		if (ReferenceEquals(this, False)) return false;

		if (Intents.Count == 0) return true;

		builder ??= new JsonSchemaBuilder();

		foreach (var intent in Intents)
		{
			intent.Apply(builder);
		}

		return builder;
	}

	internal abstract void GenerateIntents();
	
	internal string DebuggerDisplay { get; init; }
}