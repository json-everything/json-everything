using System;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	internal interface IAttributeHandler
	{
		void AddConstraints(JsonSchemaBuilder builder, IEnumerable<Attribute> attributes, Type target);
	}
}