using System;

namespace Json.Schema
{
	internal class NullLog : ILog
	{
		public static NullLog Instance { get; } = new NullLog();

		public int Indent { get; set; }

		private NullLog(){ }

		public void Write(Func<string> log) { }
	}
}