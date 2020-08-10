using System.Collections.Generic;

namespace Json.Schema.Tests.Suite
{
	public class TestCollection
	{
		public string Description { get; set; }
		public JsonSchema Schema { get; set; }
		public List<TestCase> Tests { get; set; }
		public bool IsOptional { get; set; }
	}
}
