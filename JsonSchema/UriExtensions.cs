using System;
using System.Linq;

namespace Json.Schema
{
	/// <summary>
	/// Provides extensions on the <see cref="Uri"/> type.
	/// </summary>
	public static class UriExtensions
	{
		/// <summary>
		/// Gets the Uri to the parent object.
		/// </summary>
		/// <param name="uri">The <see cref="Uri" /> of a resource, for which the parent Uri should be retrieved.</param>
		/// <returns>
		/// The parent <see cref="Uri" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="uri" /> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="uri" /> has no parent, it refers to a root resource.</exception>
		// Source: https://github.com/WebDAVSharp/WebDAVSharp.Server/blob/1d2086a502937936ebc6bfe19cfa15d855be1c31/WebDAVExtensions.cs
		public static Uri GetParentUri(this Uri uri)
		{
			if (uri == null) throw new ArgumentNullException(nameof(uri));
			if (uri.IsAbsoluteUri && uri.Segments.Length == 1) throw new InvalidOperationException("Cannot get parent of root");

			var path = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);

			return new Uri(path);
		}
	}
}