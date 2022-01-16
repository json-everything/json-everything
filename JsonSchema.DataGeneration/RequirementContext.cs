using System.Collections.Generic;

namespace Json.Schema.DataGeneration
{
	internal class RequirementContext
	{
		private const SchemaValueType _allTypes =
			SchemaValueType.Array |
			SchemaValueType.Boolean |
			SchemaValueType.Integer |
			SchemaValueType.Null |
			SchemaValueType.Number |
			SchemaValueType.Object |
			SchemaValueType.String;

		public SchemaValueType Type { get; set; } = _allTypes;

		public NumberRangeSet? NumberRanges { get; set; }
		public List<decimal>? Multiples { get; set; }
		public List<decimal>? Antimultiples { get; set; }

		public void And(RequirementContext other)
		{
			Type &= other.Type;

			if (NumberRanges == null)
				NumberRanges = other.NumberRanges;
			else if (other.NumberRanges != null)
				NumberRanges *= other.NumberRanges;

			if (Multiples == null)
				Multiples = other.Multiples;
			else if (other.Multiples != null)
				Multiples.AddRange(other.Multiples);

			if (Antimultiples == null)
				Antimultiples = other.Antimultiples;
			else if (other.Antimultiples != null)
				Antimultiples.AddRange(other.Antimultiples);
		}
	}
}