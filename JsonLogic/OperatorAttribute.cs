using System;

namespace Json.Logic
{
	/// <summary>
	/// Decorates <see cref="Rule"/> implementations to identify a rule.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class OperatorAttribute : Attribute
	{
		/// <summary>
		/// The identifier.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Creates a new <see cref="OperatorAttribute"/> instance.
		/// </summary>
		/// <param name="name">The identifier.</param>
		public OperatorAttribute(string name)
		{
			Name = name;
		}
	}
}