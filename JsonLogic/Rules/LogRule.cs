using System;
using System.Text.Json;

namespace Json.Logic.Rules
{
	[Operator("log")]
	internal class LogRule : Rule
	{
		private readonly Rule _log;

		public LogRule(Rule log)
		{
			_log = log;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var log = _log.Apply(data);

			Console.WriteLine(log);

			return data;
		}
	}
}