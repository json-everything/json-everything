using System;

namespace Json.Path
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