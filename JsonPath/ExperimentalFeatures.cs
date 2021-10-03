using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Json.Path
{
	public class ExperimentalFeatures
	{
		private Func<Uri, Task<JsonDocument?>>? _download;
		
		public bool ProcessDataReferences { get; set; }

		public Func<Uri, Task<JsonDocument?>> DataReferenceDownload
		{
			get => _download ??= ReferenceHandler.DefaultDownload;
			set => _download = value;
		}

	}
}