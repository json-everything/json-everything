using Json.Pointer;

namespace Json.Schema
{
	public class Annotation
	{
		public string Owner { get; }
		public object Value { get; }
		public JsonPointer Source { get; }

		public Annotation(string owner, object value, in JsonPointer source)
		{
			Owner = owner;
			Value = value;
			Source = source;
		}
	}
}