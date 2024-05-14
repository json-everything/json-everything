using System;
using System.Collections.Generic;
using System.Reflection;

namespace Json.Schema.Generation;

internal class MemberInfoMetadataTokenComparer<T> : Comparer<MemberInfo>
{
	private readonly Type[] _typeOrder;

	public static MemberInfoMetadataTokenComparer<T> Instance { get; } = new();

	private MemberInfoMetadataTokenComparer()
	{
		var typeStack = new Stack<Type>();
		var type = typeof(T);

		do
		{
			typeStack.Push(type);
			type = type.BaseType!;

		} while (type != null!);

		_typeOrder = [.. typeStack];
	}

	public override int Compare(MemberInfo? x, MemberInfo? y)
	{
		if (x == y) return 0;
		if (x == null) return 1;
		if (y == null) return -1;

		// Get metadata tokens for the types that declared the members.
		var xOwnerType = x.DeclaringType;
		var yOwnerType = y.DeclaringType;

		if (xOwnerType != yOwnerType)
		{
			// Members were declared in different types. Find the _typeOrder indices for
			// the types so that we can identify which one we consider to be the
			// least-derived type.
			var xIndex = Array.IndexOf(_typeOrder, xOwnerType);
			var yIndex = Array.IndexOf(_typeOrder, yOwnerType);

			// one of the types wasn't found (shouldn't happen)
			if (xIndex < 0 || yIndex < 0) return int.MaxValue;

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

#if NETSTANDARD2_0
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
#else
		return member.HasMetadataToken();
#endif
	}

	private static int GetMetadataToken(MemberInfo? member)
	{
		return HasMetadataToken(member) ? member!.MetadataToken : int.MaxValue;
	}
}