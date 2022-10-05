using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// Allows configuration of the evaluation process.
/// </summary>
public class EvaluationOptions
{
	private Uri? _defaultBaseUri;
	private ILog? _log;

	/// <summary>
	/// The default settings.
	/// </summary>
	public static EvaluationOptions Default { get; } = new();

	/// <summary>
	/// Indicates which schema draft to process as.  This will filter the keywords
	/// of a schema based on their support.
	/// </summary>
	public Draft EvaluateAs { get; set; }
	/// <summary>
	/// Indicates whether the schema should be validated against its `$schema` value.
	/// this is not typically necessary.
	/// </summary>
	public bool ValidateAgainstMetaSchema { get; set; }
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
	public VocabularyRegistry VocabularyRegistry { get; } = new();
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

	/// <summary>
	/// Specifies whether custom keywords that aren't defined in vocabularies
	/// should be processed.  Only applies to vocab-enabled JSON Schema versions
	/// (e.g. draft 2019-09 &amp; 20200-12).  Default is false.
	/// </summary>
	public bool ProcessCustomKeywords { get; set; }

	internal Draft EvaluatingAs { get; private set; }

	static EvaluationOptions()
	{
		Default.SchemaRegistry.InitializeMetaSchemas();
	}

	/// <summary>
	/// Create a new instance of the <see cref="EvaluationOptions"/> class.
	/// </summary>
	public EvaluationOptions()
	{
		SchemaRegistry = new SchemaRegistry(this);
	}

	internal static EvaluationOptions From(EvaluationOptions other)
	{
		var options = new EvaluationOptions
		{
			EvaluateAs = other.EvaluateAs,
			OutputFormat = other.OutputFormat,
			DefaultBaseUri = other.DefaultBaseUri,
			ValidateAgainstMetaSchema = other.ValidateAgainstMetaSchema,
			RequireFormatValidation = other.RequireFormatValidation,
			ProcessCustomKeywords = other.ProcessCustomKeywords,
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
		var currentlyEvaluatingAs = EvaluateAs;
		EvaluatingAs = Draft.Unspecified;
		while (metaSchemaId != null && EvaluatingAs == Draft.Unspecified)
		{
			EvaluatingAs = metaSchemaId.OriginalString switch
			{
				MetaSchemas.Draft6IdValue => Draft.Draft6,
				MetaSchemas.Draft7IdValue => Draft.Draft7,
				MetaSchemas.Draft201909IdValue => Draft.Draft201909,
				MetaSchemas.Draft202012IdValue => Draft.Draft202012,
				MetaSchemas.DraftNextIdValue => Draft.DraftNext,
				_ => currentlyEvaluatingAs
			};
			if (metaSchemaId == MetaSchemas.Draft6Id || metaSchemaId == MetaSchemas.Draft7Id)
				return DisallowSiblingRef(keywords, EvaluatingAs);
			if (metaSchemaId == MetaSchemas.Draft201909Id ||
			    metaSchemaId == MetaSchemas.Draft202012Id ||
			    metaSchemaId == MetaSchemas.DraftNextId)
				return AllowSiblingRef(keywords, EvaluatingAs);
			var metaSchema = registry.Get(metaSchemaId);
			if (metaSchema == null) return ByOption(keywords);
			var newMetaSchemaId = metaSchema.Keywords!.OfType<SchemaKeyword>().FirstOrDefault()?.Schema;
			if (newMetaSchemaId == metaSchemaId)
				throw new InvalidOperationException("Custom meta-schema `$schema` keywords must eventually resolve to a known draft meta-schema.");
			metaSchemaId = newMetaSchemaId;
		}

		if (EvaluatingAs == Draft.Unspecified)
			EvaluatingAs = currentlyEvaluatingAs;

		return ByOption(keywords);
	}

	private IEnumerable<IJsonSchemaKeyword> ByOption(IEnumerable<IJsonSchemaKeyword> keywords)
	{
		switch (EvaluateAs)
		{
			case Draft.Draft6:
			case Draft.Draft7:
				return DisallowSiblingRef(keywords, EvaluateAs);
			case Draft.Unspecified:
			case Draft.Draft201909:
			case Draft.Draft202012:
			case Draft.DraftNext:
			default:
				return AllowSiblingRef(keywords, EvaluateAs);
		}
	}

	private static IEnumerable<IJsonSchemaKeyword> DisallowSiblingRef(IEnumerable<IJsonSchemaKeyword> keywords, Draft draft)
	{
		var refKeyword = keywords.OfType<RefKeyword>().SingleOrDefault();

		return refKeyword != null ? new[] { refKeyword } : FilterByDraft(keywords, draft);
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