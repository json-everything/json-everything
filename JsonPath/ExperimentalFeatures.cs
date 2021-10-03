using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Json.Path
{
	/// <summary>
	/// Provides options for experimental features.
	/// </summary>
	/// <remarks>
	/// Changes to this object will not be reflected in the version number.
	/// </remarks>
	public class ExperimentalFeatures
	{
		private Func<Uri, Task<JsonDocument?>>? _download;
		
		/// <summary>
		/// Enables dereferencing of reference objects such as
		/// <code>{ "$ref": "http://example.com/data#/pointer/to/value" }</code>.
		/// The resulting value will then replace the reference object in the
		/// evaluation results.  This will be run after each selector in the path.
		/// </summary>
		public bool ProcessDataReferences { get; set; }

		/// <summary>
		/// Gets or sets the document download mechanism for resolving URIs.  This
		/// default is a simple, uncached download.
		/// </summary>
		public Func<Uri, Task<JsonDocument?>> DataReferenceDownload
		{
			get => _download ??= ReferenceHandler.DefaultDownload;
			set => _download = value;
		}

	}
}