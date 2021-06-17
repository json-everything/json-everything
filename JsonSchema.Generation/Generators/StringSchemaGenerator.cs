using System;

namespace Json.Schema.Generation.Generators
{
	internal class StringSchemaGenerator : BaseReferenceTypeGenerator
	{
		protected override SchemaValueType Type { get; } = SchemaValueType.String;

		public override bool Handles(Type type)
		{
			return type == typeof(string);
		}
	}
}