using System;
using System.Collections;

namespace Json.Schema.Generation
{
	internal static class TypeExtensions
	{
		public static bool IsInteger(this Type type)
		{
			return type == typeof(byte) ||
			       type == typeof(short) ||
			       type == typeof(ushort) ||
			       type == typeof(int) ||
			       type == typeof(uint) ||
			       type == typeof(long) ||
			       type == typeof(ulong);
		}
		
		public static bool IsFloatingPoint(this Type type)
		{
			return type == typeof(float) ||
			       type == typeof(double) ||
			       type == typeof(decimal);
		}

		public static bool IsNumber(this Type type)
		{
			return type.IsInteger() || type.IsFloatingPoint();
		}

		public static bool IsArray(this Type type)
		{
			return type.IsArray ||
			       type == typeof(Array) ||
			       typeof(IEnumerable).IsAssignableFrom(type);
		}
	}
}
