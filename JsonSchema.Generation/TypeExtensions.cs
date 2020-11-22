using System;

namespace Json.Schema.Generation
{
	internal static class TypeExtensions
	{
		public static bool IsNumber(this Type type)
		{
			return type == typeof(byte) ||
			       type == typeof(short) ||
			       type == typeof(ushort) ||
			       type == typeof(int) ||
			       type == typeof(uint) ||
			       type == typeof(long) ||
			       type == typeof(ulong) ||
			       type == typeof(float) ||
			       type == typeof(double) ||
			       type == typeof(decimal);
		}
	}
}
