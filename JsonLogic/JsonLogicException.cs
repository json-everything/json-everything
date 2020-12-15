using System;

namespace Json.Logic
{
	public class JsonLogicException : Exception
	{
		public JsonLogicException(string message)
			: base(message)
		{
		}
	}
}