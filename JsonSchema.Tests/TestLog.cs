using System;

namespace Json.Schema.Tests
{
	public class TestLog : ILog
	{
		public void Write(Func<string> log)
		{
			Console.WriteLine(log());
		}
	}
}