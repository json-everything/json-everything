using System;

namespace Json.Schema
{
	/// <summary>
	/// Enumerates the supported JSON Schema drafts.
	/// </summary>
	[Flags]
	public enum Draft
	{
		/// <summary>
		/// The draft to use should be determined by the collection of keywords.
		/// </summary>
		Unspecified,
		/// <summary>
		/// JSON Schema Draft 6.
		/// </summary>
		Draft6 = 1,
		/// <summary>
		/// JSON Schema Draft 7.
		/// </summary>
		Draft7 = 1 << 1,
		/// <summary>
		/// JSON Schema Draft 2019-09.
		/// </summary>
		Draft201909 = 1 << 2,
		/// <summary>
		/// JSON Schema Draft 2020-12.
		/// </summary>
		Draft202012 = 1 << 3
	}
}