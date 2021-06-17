using Json.Schema.Generation.Intents;
using System;

namespace Json.Schema.Generation.Generators
{
	internal class UriSchemaGenerator : BaseReferenceTypeGenerator
	{
		protected override SchemaValueType Type { get; } = SchemaValueType.String;

		public override bool Handles(Type type)
		{
			return type == typeof(Uri);
		}

		public override void AddConstraints(SchemaGeneratorContext context)
		{
			base.AddConstraints(context);

			context.Intents.Add(new FormatIntent(Formats.Uri));
		}
	}
}