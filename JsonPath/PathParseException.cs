using System;

namespace JsonPath
{
	public class PathParseException : Exception
	{
		public int Index { get; }

		internal PathParseException(int index, string message)
			: base(message)
		{
			Index = index;
		}
	}
}