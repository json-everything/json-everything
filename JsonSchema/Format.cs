using System.Text.Json;

namespace Json.Schema
{
	public class Format
	{
		public string Key { get; }

		internal bool IsUnknown => Key == null;

		internal Format(){}
		public Format(string key)
		{
			Key = key;
		}

		public virtual bool Validate(JsonElement element)
		{
			return true;
		}
	}
}