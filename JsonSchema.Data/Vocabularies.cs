namespace Json.Schema.Data
{
	public static class Vocabularies
	{
		public const string DataId = "https://gregsdennis.github.io/json-everything/vocabs-data";

		public static readonly Vocabulary Data = new Vocabulary(DataId, typeof(DataKeyword));
	}
}