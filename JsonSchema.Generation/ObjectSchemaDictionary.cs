using System;

namespace Json.Schema.Generation
{
	internal class ObjectSchemaDictionary : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			throw new NotImplementedException();
		}

		public void AddConstraints(JsonSchemaBuilder builder, Type type)
		{
			throw new NotImplementedException();
		}
	}
}