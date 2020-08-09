using System;

namespace Json.Pointer
{
	public static class CharExtensions
	{
		public static bool IsHexadecimal(this char c)
		{
			return (c >= '0' && c <= '9') || 
			       (c >= 'a' && c <= 'f') ||
			       (c >= 'A' && c <= 'F');
		}

		public static int GetHexadecimalValue(this char c)
		{
			return c switch
				{
					{ } when c >= '0' && c <= '9' => c - '0',
					{ } when c >= 'a' && c <= 'f' => c - 'a' + 10,
					{ } when c >= 'A' && c <= 'F' => c - 'A' + 10,
					_ => throw new ArgumentOutOfRangeException($"`{c}` is not a valid hexadecimal character")
				};
		}
	}
}