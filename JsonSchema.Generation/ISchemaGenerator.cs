using System;

namespace Json.Schema.Generation
{
	public interface ISchemaGenerator
	{
		bool Handles(Type type);
		void AddConstraints(SchemaGeneratorContext context);
	}
}