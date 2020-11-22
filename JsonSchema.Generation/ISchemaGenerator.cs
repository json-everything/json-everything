using System;

namespace Json.Schema.Generation
{
	internal interface ISchemaGenerator
	{
		bool Handles(Type type);
		void AddConstraints(JsonSchemaBuilder builder, Type type);
	}
}