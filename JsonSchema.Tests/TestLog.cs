using System;

namespace Json.Schema.Tests
{
	public class TestLog : ILog
	{
		public void Log(Func<string> log)
		{
			Console.WriteLine(log());
		}
	}
}