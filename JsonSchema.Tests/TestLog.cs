using System;
using System.Linq;

namespace Json.Schema.Tests
{
	public class TestLog : ILog
	{
		public int Indent { get; set; }

		public void Write(Func<string> log)
		{
			var indent = string.Concat(Enumerable.Repeat("  ", Indent));
			Console.WriteLine($"{indent}{log()}");
		}
	}
}