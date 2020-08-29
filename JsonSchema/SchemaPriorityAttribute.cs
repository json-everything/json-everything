using System;

namespace Json.Schema
{
	/// <summary>
	/// Indicates keyword priority.
	/// </summary>
	/// <remarks>
	/// Keywords are processed in priority order.  This will help process multiple
	/// keywords in the proper sequence.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class SchemaPriorityAttribute : Attribute
	{
		private readonly long? _actualPriority;

		/// <summary>
		/// The keyword priority.
		/// </summary>
		public int Priority { get; }

		internal long ActualPriority => _actualPriority ?? Priority;

		/// <summary>
		/// Creates a new <see cref="SchemaPriorityAttribute"/>.
		/// </summary>
		/// <param name="priority">The keyword priority.</param>
		public SchemaPriorityAttribute(int priority)
		{
			Priority = priority;
		}

		internal SchemaPriorityAttribute(long actualPriority)
		{
			_actualPriority = actualPriority;
		}
	}
}