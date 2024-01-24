#if NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis
{
	internal class NotNullWhenAttribute : Attribute
	{
		public NotNullWhenAttribute(bool value) { }
	}
}

#endif