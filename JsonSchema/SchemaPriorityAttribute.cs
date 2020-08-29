using System;

namespace Json.Schema
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class SchemaPriorityAttribute : Attribute
	{
		private readonly long? _actualPriority;

		public int Priority { get; }

		internal long ActualPriority => _actualPriority ?? Priority;

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