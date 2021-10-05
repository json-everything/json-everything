using System.Collections.Generic;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	public class LibraryVersionModel
	{
		public string Name { get; set; }
		public string Version { get; set; }
	}

	public class LibraryVersionCollectionModel : List<LibraryVersionModel>
	{
		public int ColumnOffset => (6 - Count) / 2;
	}
}
