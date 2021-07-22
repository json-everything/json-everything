using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema.Generation
{
	internal class MemberInfoMetadataTokenComparer : Comparer<MemberInfo>
	{
		private readonly int[] _typeOrder;

		internal MemberInfoMetadataTokenComparer(Type type)
		{
			var typeStack = new Stack<Type>();

			do
			{
				typeStack.Push(type);
				type = type.BaseType!;
			} while (type != null!);

			_typeOrder = typeStack.Select(GetMetadataToken).ToArray();
		}

		public override int Compare(MemberInfo? x, MemberInfo? y)
		{
			if (x == y) return 0;
			if (x == null) return 1;
			if (y == null) return -1;

			// Get metadata tokens for the types that declared the members.
			var xTypeToken = GetMetadataToken(x.DeclaringType);
			var yTypeToken = GetMetadataToken(y.DeclaringType);

			if (xTypeToken != yTypeToken)
			{
				// Members were declared in different types. Find the _typeOrder indices for
				// the types so that we can identify which one we consider to be the
				// least-derived type.
				var xIndex = Array.IndexOf(_typeOrder, xTypeToken);
				var yIndex = Array.IndexOf(_typeOrder, yTypeToken);

				if (xIndex < 0 && yIndex < 0) return Comparer<int>.Default.Compare(xTypeToken, yTypeToken);
				if (xIndex < 0) return 1;
				if (yIndex < 0) return -1;

				return Comparer<int>.Default.Compare(xIndex, yIndex);
			}

			// Members were declared in the same type. Use the metadata tokens for the members
			// to determine the sort order.
			var xToken = GetMetadataToken(x);
			var yToken = GetMetadataToken(y);

			return Comparer<int>.Default.Compare(xToken, yToken);
		}

		private static bool HasMetadataToken(MemberInfo? member)
		{
			if (member == null) return false;

#if NET5_0
				return member.HasMetadataToken();
#else
			try
			{
				// ReSharper disable once UnusedVariable
				var token = member.MetadataToken;
				return true;
			}
			catch (InvalidOperationException)
			{
				return false;
			}
#endif
		}

		private static int GetMetadataToken(MemberInfo? member)
		{
			return HasMetadataToken(member) ? member!.MetadataToken : int.MaxValue;
		}
	}
}
