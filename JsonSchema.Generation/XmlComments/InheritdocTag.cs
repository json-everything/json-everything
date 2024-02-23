namespace Json.Schema.Generation.XmlComments;

/// <summary>
///     Inheritdoc tag with optional cref attribute.
/// </summary>
public class InheritdocTag
{
	public InheritdocTag(string cref)
	{
		Cref = cref;
	}

	/// <summary>
	///     Cref attribute value. This value is optional.
	/// </summary>
	public string Cref { get; set; }
}
