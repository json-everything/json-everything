namespace Json.Schema.Generation;

/// <summary>
/// Indicates an attribute can support being applied to generic parameters.
/// </summary>
public interface INestableAttribute
{
	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// Default MUST be -1, which indicates the root type.
	/// For example, `Person` in `Dictionary&lt;string, Person&gt;` would have a parameter of 1.
	/// </summary>
	int GenericParameter { get; set; }
}