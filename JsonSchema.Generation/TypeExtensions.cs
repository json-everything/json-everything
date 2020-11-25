using System;
using System.Collections;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	public static class TypeExtensions
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

		internal static int GetAttributeSetHashCode(this IEnumerable<Attribute> items)
		{
			unchecked
			{
				int hashCode = 0;
				foreach (var item in items)
				{
					hashCode = (hashCode * 397) ^ item.GetHashCode();
				}
				return hashCode;
			}
		}
	}
}
