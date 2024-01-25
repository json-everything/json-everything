#if NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis
{
	internal class NotNullWhenAttribute : Attribute
	{
		public NotNullWhenAttribute(bool value) { }
	}

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	internal class UnconditionalSuppressMessage : Attribute
	{
		public string Category { get; } = null!;
        public string CheckId { get; } = null!;
		public string? Scope { get; set; }
        public string? Target { get; set; }
        public string? MessageId { get; set; }
        public string? Justification { get; set; }

		public UnconditionalSuppressMessage(string category, string checkId)
		{

		}
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
	internal sealed class RequiresUnreferencedCodeAttribute : Attribute
	{
		public string Message { get; }
		public string? Url { get; set; }

		public RequiresUnreferencedCodeAttribute(string message)
		{
			Message = message;
		}
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
	internal sealed class RequiresDynamicCodeAttribute : Attribute
	{
		public string Message { get; }
		public string? Url { get; set; }

		public RequiresDynamicCodeAttribute(string message)
		{
			Message = message;
		}
	}

	[AttributeUsage(
		AttributeTargets.Field | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter |
		AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Method |
		AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
		Inherited = false)]
	internal sealed class DynamicallyAccessedMembersAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicallyAccessedMembersAttribute"/> class
		/// with the specified member types.
		/// </summary>
		/// <param name="memberTypes">The types of members dynamically accessed.</param>
		public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
		{
			MemberTypes = memberTypes;
		}

		/// <summary>
		/// Gets the <see cref="DynamicallyAccessedMemberTypes"/> which specifies the type
		/// of members dynamically accessed.
		/// </summary>
		public DynamicallyAccessedMemberTypes MemberTypes { get; }
	}

	[Flags]
	internal enum DynamicallyAccessedMemberTypes
	{
		/// <summary>
		/// Specifies no members.
		/// </summary>
		None = 0,

		/// <summary>
		/// Specifies the default, parameterless public constructor.
		/// </summary>
		PublicParameterlessConstructor = 0x0001,

		/// <summary>
		/// Specifies all public constructors.
		/// </summary>
		PublicConstructors = 0x0002 | PublicParameterlessConstructor,

		/// <summary>
		/// Specifies all non-public constructors.
		/// </summary>
		NonPublicConstructors = 0x0004,

		/// <summary>
		/// Specifies all public methods.
		/// </summary>
		PublicMethods = 0x0008,

		/// <summary>
		/// Specifies all non-public methods.
		/// </summary>
		NonPublicMethods = 0x0010,

		/// <summary>
		/// Specifies all public fields.
		/// </summary>
		PublicFields = 0x0020,

		/// <summary>
		/// Specifies all non-public fields.
		/// </summary>
		NonPublicFields = 0x0040,

		/// <summary>
		/// Specifies all public nested types.
		/// </summary>
		PublicNestedTypes = 0x0080,

		/// <summary>
		/// Specifies all non-public nested types.
		/// </summary>
		NonPublicNestedTypes = 0x0100,

		/// <summary>
		/// Specifies all public properties.
		/// </summary>
		PublicProperties = 0x0200,

		/// <summary>
		/// Specifies all non-public properties.
		/// </summary>
		NonPublicProperties = 0x0400,

		/// <summary>
		/// Specifies all public events.
		/// </summary>
		PublicEvents = 0x0800,

		/// <summary>
		/// Specifies all non-public events.
		/// </summary>
		NonPublicEvents = 0x1000,

		/// <summary>
		/// Specifies all interfaces implemented by the type.
		/// </summary>
		Interfaces = 0x2000,

		/// <summary>
		/// Specifies all members.
		/// </summary>
		All = ~None
	}

}

#endif