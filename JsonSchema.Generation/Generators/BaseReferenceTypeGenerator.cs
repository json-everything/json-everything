using Json.Schema.Generation.Intents;
using System;

namespace Json.Schema.Generation.Generators
{
	internal abstract class BaseReferenceTypeGenerator : ISchemaGenerator
	{
		public abstract bool Handles(Type type);

		protected abstract SchemaValueType Type { get; }

		public virtual void AddConstraints(SchemaGeneratorContext context)
		{
			var valueType = Type;

			if ((context.Configuration.Nullability & Nullability.AllowForReferenceTypes) != 0)
			{
				valueType |= SchemaValueType.Null;
			}

			context.Intents.Add(new TypeIntent(valueType));
		}

	}
}
