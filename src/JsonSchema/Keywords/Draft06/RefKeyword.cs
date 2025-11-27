namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `$ref` for Draft 6 & Draft 7 by ignoring sibling keywords.
/// </summary>
public class RefKeyword : Keywords.RefKeyword
{
	public override bool IgnoresSiblingKeywords => true;
}
