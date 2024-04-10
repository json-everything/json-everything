namespace Json.Schema;

/// <summary>
/// Exposes the meta-schemas for the supported drafts.
/// </summary>
public static partial class MetaSchemas
{
	static MetaSchemas()
	{
		SchemaRegistry.Global.Register(Draft6Id, Draft6);

		SchemaRegistry.Global.Register(Draft7Id, Draft7);

		SchemaRegistry.Global.Register(Draft201909Id, Draft201909);
		SchemaRegistry.Global.Register(Core201909Id, Core201909);
		SchemaRegistry.Global.Register(Applicator201909Id, Applicator201909);
		SchemaRegistry.Global.Register(Validation201909Id, Validation201909);
		SchemaRegistry.Global.Register(Metadata201909Id, Metadata201909);
		SchemaRegistry.Global.Register(Format201909Id, Format201909);
		SchemaRegistry.Global.Register(Content201909Id, Content201909);

		SchemaRegistry.Global.Register(Draft202012Id, Draft202012);
		SchemaRegistry.Global.Register(Core202012Id, Core202012);
		SchemaRegistry.Global.Register(Unevaluated202012Id, Unevaluated202012);
		SchemaRegistry.Global.Register(Applicator202012Id, Applicator202012);
		SchemaRegistry.Global.Register(Validation202012Id, Validation202012);
		SchemaRegistry.Global.Register(Metadata202012Id, Metadata202012);
		SchemaRegistry.Global.Register(FormatAnnotation202012Id, FormatAnnotation202012);
		SchemaRegistry.Global.Register(FormatAssertion202012Id, FormatAssertion202012);
		SchemaRegistry.Global.Register(Content202012Id, Content202012);

		SchemaRegistry.Global.Register(DraftNextId, DraftNext);
		SchemaRegistry.Global.Register(CoreNextId, CoreNext);
		SchemaRegistry.Global.Register(UnevaluatedNextId, UnevaluatedNext);
		SchemaRegistry.Global.Register(ApplicatorNextId, ApplicatorNext);
		SchemaRegistry.Global.Register(ValidationNextId, ValidationNext);
		SchemaRegistry.Global.Register(MetadataNextId, MetadataNext);
		SchemaRegistry.Global.Register(FormatAnnotationNextId, FormatAnnotationNext);
		SchemaRegistry.Global.Register(FormatAssertionNextId, FormatAssertionNext);
		SchemaRegistry.Global.Register(ContentNextId, ContentNext);
	}
}