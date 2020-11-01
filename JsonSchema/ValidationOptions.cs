using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema
{
	/// <summary>
	/// Allows configuration of the validation process.
	/// </summary>
	public class ValidationOptions
	{
		private Uri _defaultBaseUri;

		/// <summary>
		/// The default settings.
		/// </summary>
		public static ValidationOptions Default { get; } = new ValidationOptions();

		/// <summary>
		/// Indicates which schema draft to process as.  This will filter the keywords
		/// of a schema based on their support.
		/// </summary>
		public Draft ValidateAs { get; set; }
		/// <summary>
		/// Indicates whether the schema should be validated against its `$schema` value.
		/// this is not typically necessary.
		/// </summary>
		public bool ValidateMetaSchema { get; set; }
		/// <summary>
		/// Specifies the output format.
		/// </summary>
		public OutputFormat OutputFormat { get; set; }
		/// <summary>
		/// The local schema registry.  If a schema is not found here, it will
		/// automatically check the global registry as well.
		/// </summary>
		public SchemaRegistry SchemaRegistry { get; } = new SchemaRegistry();
		/// <summary>
		/// The local vocabulary registry.  If a schema is not found here, it will
		/// automatically check the global registry as well.
		/// </summary>
		public VocabularyRegistry VocabularyRegistry { get; } = new VocabularyRegistry();
		/// <summary>
		/// Specifies a default URI to be used when a schema is missing a
		/// </summary>
		public Uri DefaultBaseUri
		{
			get => _defaultBaseUri ??= new Uri("https://json-everything/base");
			set => _defaultBaseUri = value;
		}
		/// <summary>
		/// Specifies whether the `format` keyword should provide validation results
		/// or just annotation.  Default is false, which just produces annotations.
		/// </summary>
		public bool ValidateFormat { get; set; }

		internal static ValidationOptions From(ValidationOptions other)
		{
			var options = new ValidationOptions
			{
				ValidateAs = other.ValidateAs,
				OutputFormat = other.OutputFormat
			};
			options.SchemaRegistry.CopyFrom(other.SchemaRegistry);
			options.VocabularyRegistry.CopyFrom(other.VocabularyRegistry);
			return options;
		}

		internal IEnumerable<IJsonSchemaKeyword> FilterKeywords(IEnumerable<IJsonSchemaKeyword> keywords, Uri metaSchemaId, SchemaRegistry registry)
		{
			Draft draft = Draft.Unspecified;
			while (metaSchemaId != null && draft == Draft.Unspecified)
			{
				draft = metaSchemaId.OriginalString switch
				{
					MetaSchemas.Draft6IdValue => Draft.Draft6,
					MetaSchemas.Draft7IdValue => Draft.Draft7,
					MetaSchemas.Draft201909IdValue => Draft.Draft201909,
					_ => Draft.Unspecified
				};
				if (metaSchemaId == MetaSchemas.Draft6Id || metaSchemaId == MetaSchemas.Draft7Id)
					return DisallowSiblingRef(keywords, draft);
				if (metaSchemaId == MetaSchemas.Draft201909Id)
					return AllowSiblingRef(keywords, draft);
				var metaSchema = registry.Get(metaSchemaId);
				if (metaSchema == null) return ByOption(keywords);
				metaSchemaId = metaSchema.Keywords.OfType<SchemaKeyword>().FirstOrDefault()?.Schema;
			}

			return ByOption(keywords);
		}

		private IEnumerable<IJsonSchemaKeyword> ByOption(IEnumerable<IJsonSchemaKeyword> keywords)
		{
			switch (ValidateAs)
			{
				case Draft.Draft6:
				case Draft.Draft7:
					return DisallowSiblingRef(keywords, ValidateAs);
				case Draft.Unspecified:
				case Draft.Draft201909:
				default:
					return AllowSiblingRef(keywords, ValidateAs);
			}
		}

		private static IEnumerable<IJsonSchemaKeyword> DisallowSiblingRef(IEnumerable<IJsonSchemaKeyword> keywords, Draft draft)
		{
			var refKeyword = keywords.OfType<RefKeyword>().SingleOrDefault();

			return refKeyword != null ? new[] {refKeyword} : FilterByDraft(keywords, draft);
		}

		private static IEnumerable<IJsonSchemaKeyword> AllowSiblingRef(IEnumerable<IJsonSchemaKeyword> keywords, Draft draft)
		{
			return FilterByDraft(keywords, draft);
		}

		private static IEnumerable<IJsonSchemaKeyword> FilterByDraft(IEnumerable<IJsonSchemaKeyword> keywords, Draft draft)
		{
			if (draft == Draft.Unspecified) return keywords;

			return keywords.Where(k => k.GetType().GetCustomAttributes<SchemaDraftAttribute>().Any(a => a.Draft == draft));
		}
	}
}