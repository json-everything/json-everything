using System;
using System.Text.Json;

namespace Json.Schema
{
	public class PredicateFormat : Format
	{
		private readonly Func<JsonElement, bool> _predicate;

		public PredicateFormat(string key, Func<JsonElement, bool> predicate)
			: base(key)
		{
			_predicate = predicate;
		}

		public override bool Validate(JsonElement element)
		{
			return _predicate(element);
		}
	}
}