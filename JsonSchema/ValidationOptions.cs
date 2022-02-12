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
		private Uri? _defaultBaseUri;
		private ILog? _log;

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
		public SchemaRegistry SchemaRegistry { get; }

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
		/// Gets or sets the indent level for the log.
		/// </summary>
		public int LogIndentLevel { get; set; }

		/// <summary>
		/// Gets or sets a log which will output processing information.
		/// </summary>
		public ILog Log
		{
			get => _log ?? NullLog.Instance;
			set => _log = value;
		}

		/// <summary>
		/// Specifies whether the `format` keyword should be required to provide
		/// validation results.  Default is false, which just produces annotations
		/// for drafts 2019-09 and prior or follows the behavior set forth by the
		/// format-annotation vocabulary requirement in the `$vocabulary` keyword in
		/// a meta-schema declaring draft 2020-12.
		/// </summary>
		public bool RequireFormatValidation { get; set; }

		/// <summary>
		/// Specifies whether the `format` keyword should fail validations for
		/// unknown formats.  Default is false.
		/// </summary>
		/// <remarks>
		///	This option is applied whether `format` is using annotation or
		/// assertion behavior.
		/// </remarks>
		public bool OnlyKnownFormats { get; set; }
		
		internal Draft ValidatingAs { get; private set; }

		static ValidationOptions()
		{
			Default.SchemaRegistry.InitializeMetaSchemas();
		}

		/// <summary>
		/// Create a new instance of the <see cref="ValidationOptions"/> class.
		/// </summary>
		public ValidationOptions()
		{
			SchemaRegistry = new SchemaRegistry(this);
		}

		internal static ValidationOptions From(ValidationOptions other)
		{
			var options = new ValidationOptions
			{
				ValidateAs = other.ValidateAs,
				OutputFormat = other.OutputFormat,
				DefaultBaseUri = other.DefaultBaseUri,
				ValidateMetaSchema = other.ValidateMetaSchema,
				RequireFormatValidation = other.RequireFormatValidation,
				LogIndentLevel = other.LogIndentLevel,
				Log = other._log ?? Default.Log,
				OnlyKnownFormats = other.OnlyKnownFormats,
			};
			options.SchemaRegistry.CopyFrom(other.SchemaRegistry);
			options.VocabularyRegistry.CopyFrom(other.VocabularyRegistry);
			return options;
		}

		internal IEnumerable<IJsonSchemaKeyword> FilterKeywords(IEnumerable<IJsonSchemaKeyword> keywords, Uri? metaSchemaId, SchemaRegistry registry)
		{
			var currentlyValidatingAs = ValidatingAs;
			ValidatingAs = Draft.Unspecified;
			while (metaSchemaId != null && ValidatingAs == Draft.Unspecified)
			{
				ValidatingAs = metaSchemaId.OriginalString switch
				{
					MetaSchemas.Draft6IdValue => Draft.Draft6,
					MetaSchemas.Draft7IdValue => Draft.Draft7,
					MetaSchemas.Draft201909IdValue => Draft.Draft201909,
					MetaSchemas.Draft202012IdValue => Draft.Draft202012,
					_ => currentlyValidatingAs
				};
				if (metaSchemaId == MetaSchemas.Draft6Id || metaSchemaId == MetaSchemas.Draft7Id)
					return DisallowSiblingRef(keywords, ValidatingAs);
				if (metaSchemaId == MetaSchemas.Draft201909Id || metaSchemaId == MetaSchemas.Draft202012Id)
					return AllowSiblingRef(keywords, ValidatingAs);
				var metaSchema = registry.Get(metaSchemaId);
				if (metaSchema == null) return ByOption(keywords);
				var newMetaSchemaId = metaSchema.Keywords!.OfType<SchemaKeyword>().FirstOrDefault()?.Schema;
				if (newMetaSchemaId == metaSchemaId)
					throw new InvalidOperationException("Custom meta-schema `$schema` keywords must eventually resolve to a known draft meta-schema.");
				metaSchemaId = newMetaSchemaId;
			}

			if (ValidatingAs == Draft.Unspecified)
				ValidatingAs = currentlyValidatingAs;

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
				case Draft.Draft202012:
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

			return keywords.Where(k => k.SupportsDraft(draft));
		}
	}
}