using System;
using System.Linq;

namespace Json.Schema.Tests
{
	public class TestLog : ILog
	{
		public void Write(Func<string> log, int indent)
		{
			var indentString = indent == 0 ? null : string.Concat(Enumerable.Repeat("  ", indent));
			Console.WriteLine($"{indentString}{log()}");
		}
	}
}