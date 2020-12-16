namespace Json.Path.Tests.Suite
{
	public class CburgmerTestCase
	{
		public string TestName { get; set; }
		public string PathString { get; set; }
		public string JsonString { get; set; }
		public string Consensus { get; set; }

		public override string ToString()
		{
			return $"TestName:   {TestName}\n" +
			       $"PathString: {PathString}\n" +
			       $"JsonString: {JsonString}\n" +
			       $"Consensus:   {Consensus}";
		}
	}
}