using System;
using System.Globalization;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.JsonE.Operators;

namespace Json.JsonE.Functions;

internal class FromNowFunction : FunctionDefinition
{
	private struct Interval
	{
		public int? Days { get; private set; }
		public int? Hours { get; private set; }
		public int? Minutes { get; private set; }
		public int? Seconds { get; private set; }

		public void Set(string field, int value)
		{
			switch (field.ToLowerInvariant())
			{
				case "day":
				case "days":
					if (Days.HasValue)
						throw new InterpreterException("Cannot set days more than once");
			
					Days = value;
					break;
				case "hour":
				case "hours":
					if (Hours.HasValue)
						throw new InterpreterException("Cannot set hours more than once");

					Hours = value;
					break;
				case "minute":
				case "minutes":
					if (Minutes.HasValue)
						throw new InterpreterException("Cannot set minutes more than once");

					Minutes = value;
					break;
				case "second":
				case "seconds":
					if (Seconds.HasValue)
						throw new InterpreterException("Cannot set seconds more than once");

					Seconds = value;
					break;
			}
		}

		public readonly TimeSpan ToTimeSpan()
		{
			return new TimeSpan(Days ?? 0, Hours ?? 0, Minutes ?? 0, Seconds ?? 0);
		}
	}

	public override string Name => "fromNow";
	public override FunctionValueType[] ParameterTypes { get; } = { FunctionValueType.String, FunctionValueType.String };
	public override FunctionValueType ReturnType => FunctionValueType.String;

	internal override JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context)
	{
		if (arguments[0] is not JsonValue val || !val.TryGetValue(out string? str))
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		string? argNowStr = null;
		if (arguments.Length == 2 && (arguments[1] is not JsonValue argNowNode ||
		                              !argNowNode.TryGetValue(out argNowStr)))
			throw new InterpreterException(CommonErrors.IncorrectArgType(Name));

		DateTime now;
		var interval = ParseAndGetTimeSpan(str);
		if (argNowStr != null)
		{
			if (!DateTime.TryParse(argNowStr, out now))
				throw new InterpreterException(CommonErrors.IncorrectArgType(Name));
		}
		else
		{
			var nowNode = context.Find(ContextAccessor.Now);
			if (nowNode is not JsonValue nowVal ||
			    !nowVal.TryGetValue(out str) ||
			    !DateTime.TryParse(str, out now))
				throw new InterpreterException(CommonErrors.IncorrectArgType(Name));
		}

		return now.ToUniversalTime().Add(interval).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFK", CultureInfo.InvariantCulture);
	}

	private static TimeSpan ParseAndGetTimeSpan(string source)
	{
		var parts = source.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length % 2 != 0)
			throw new InterpreterException("Argument is invalid");

		var interval = new Interval();
		for (int i = 0; i < parts.Length; i+= 2)
		{
			if (!int.TryParse(parts[i], out var value))
				throw new InterpreterException("Argument is invalid");

			interval.Set(parts[i+1], value);
		}

		return interval.ToTimeSpan();
	}
}
