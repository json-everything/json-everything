namespace Json.Patch
{
	internal class PatchContext
	{
		public EditableJsonElement Source { get; set; }
		public string? Message { get; set; }
		public int Index { get; set; }

		public PatchContext(EditableJsonElement source)
		{
			Source = source;
		}
	}
}
