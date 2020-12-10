namespace Json.Pointer
{
	/// <summary>
	/// Enumerates the different styles of JSON pointers.
	/// </summary>
	public enum JsonPointerKind
	{
		/// <summary>
		/// No format specified.
		/// </summary>
		Unspecified,
		/// <summary>
		/// Indicates only plain JSON pointers.
		/// </summary>
		Plain,
		/// <summary>
		/// Indicates only URI-encoded JSON pointers.
		/// </summary>
		UriEncoded
	}
}
