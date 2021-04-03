using System;

namespace Json.Schema
{
	/// <summary>
	/// Used to log processing details.
	/// </summary>
	public interface ILog
	{
		void Log(Func<string> log);
	}
}