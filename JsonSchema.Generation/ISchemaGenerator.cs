using System;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	public interface ISchemaGenerator
	{
		bool Handles(Type type);
		void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context);
	}
}