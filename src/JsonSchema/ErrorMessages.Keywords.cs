using System.Globalization;
using Json.Schema.Keywords;

namespace Json.Schema;

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for the "false" schema.
	/// </summary>
	/// <remarks>No tokens are supported.</remarks>
	public static string FalseSchema
	{
		get => field ?? Get();
		set;
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="ConstKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string? Const { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="ConstKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string GetConst(CultureInfo? culture)
	{
		return Const ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="ContainsKeyword"/> when there are too few matching items.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of matching items provided in the JSON instance
	///   - [[minimum]] - the lower limit specified in the schema
	/// </remarks>
	public static string? ContainsTooFew { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="ContainsKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of matching items provided in the JSON instance
	///   - [[minimum]] - the lower limit specified in the schema
	/// </remarks>
	public static string GetContainsTooFew(CultureInfo? culture)
	{
		return ContainsTooFew ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="ContainsKeyword"/> when there are too many matching items.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of matching items provided in the JSON instance
	///   - [[maximum]] - the upper limit specified in the schema
	/// </remarks>
	public static string? ContainsTooMany { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="ContainsKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of matching items provided in the JSON instance
	///   - [[maximum]] - the upper limit specified in the schema
	/// </remarks>
	public static string GetContainsTooMany(CultureInfo? culture)
	{
		return ContainsTooMany ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="DependentRequiredKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the value in the schema
	/// </remarks>
	public static string? DependentRequired { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="DependentRequiredKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the value in the schema
	/// </remarks>
	public static string GetDependentRequired(CultureInfo? culture)
	{
		return DependentRequired ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="DependentSchemasKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string? DependentSchemas { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="DependentSchemasKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[value]] - the value in the schema
	/// </remarks>
	public static string GetDependentSchemas(CultureInfo? culture)
	{
		return DependentSchemas ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="EnumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[values]] - the available values in the schema
	///
	/// The default messages are static and do not use these tokens as enum values
	/// may be any JSON type and could be quite large.  They are provided to support
	/// custom messages.
	/// </remarks>
	public static string? Enum { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="EnumKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[values]] - the available values in the schema
	///
	/// The default messages are static and do not use these tokens as enum values
	/// may be any JSON type and could be quite large.  They are provided to support
	/// custom messages.
	/// </remarks>
	public static string GetEnum(CultureInfo? culture)
	{
		return Enum ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="ExclusiveMaximumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the upper limit in the schema
	/// </remarks>
	public static string? ExclusiveMaximum { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="ExclusiveMaximumKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the upper limit in the schema
	/// </remarks>
	public static string GetExclusiveMaximum(CultureInfo? culture)
	{
		return ExclusiveMaximum ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="ExclusiveMinimumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the lower limit in the schema
	/// </remarks>
	public static string? ExclusiveMinimum { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="ExclusiveMinimumKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the lower limit in the schema
	/// </remarks>
	public static string GetExclusiveMinimum(CultureInfo? culture)
	{
		return ExclusiveMinimum ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for an unknown format.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	/// </remarks>
	public static string? UnknownFormat { get; set; }

	/// <summary>
	/// Gets the error message for an unknown format.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	/// </remarks>
	public static string GetUnknownFormat(CultureInfo? culture)
	{
		return UnknownFormat ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for the <see cref="FormatKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	/// </remarks>
	public static string? Format { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="FormatKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	/// </remarks>
	public static string GetFormat(CultureInfo? culture)
	{
		return Format ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for the <see cref="FormatKeyword"/> with
	/// additional information from the format validation.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	///   - [[detail]] - the detail
	/// </remarks>
	public static string? FormatWithDetail { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="FormatKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	///   - [[detail]] - the detail
	/// </remarks>
	public static string GetFormatWithDetail(CultureInfo? culture)
	{
		return FormatWithDetail ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="MinimumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the upper limit in the schema
	/// </remarks>
	public static string? Maximum { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MinimumKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the upper limit in the schema
	/// </remarks>
	public static string GetMaximum(CultureInfo? culture)
	{
		return Maximum ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="MaxItemsKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of items provided in the JSON instance
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string? MaxItems { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MaxItemsKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of items provided in the JSON instance
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string GetMaxItems(CultureInfo? culture)
	{
		return MaxItems ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="MaxLengthKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the length of the JSON string
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string? MaxLength { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MaxLengthKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the length of the JSON string
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string GetMaxLength(CultureInfo? culture)
	{
		return MaxLength ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="MaxPropertiesKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string? MaxProperties { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MaxPropertiesKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string GetMaxProperties(CultureInfo? culture)
	{
		return MaxProperties ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="MinimumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the lower limit in the schema
	/// </remarks>
	public static string? Minimum { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MinimumKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture"></param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the lower limit in the schema
	/// </remarks>
	public static string GetMinimum(CultureInfo? culture)
	{
		return Minimum ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="MinItemsKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of items provided in the JSON instance
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string? MinItems { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MinItemsKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of items provided in the JSON instance
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string GetMinItems(CultureInfo? culture)
	{
		return MinItems ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="MinLengthKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the length of the JSON string
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string? MinLength { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MinLengthKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the length of the JSON string
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string GetMinLength(CultureInfo? culture)
	{
		return MinLength ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="MinPropertiesKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string? MinProperties { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MinPropertiesKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string GetMinProperties(CultureInfo? culture)
	{
		return MinProperties ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="MultipleOfKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[divisor]] - the required divisor
	/// </remarks>
	public static string? MultipleOf { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MultipleOfKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[divisor]] - the required divisor
	/// </remarks>
	public static string GetMultipleOf(CultureInfo? culture)
	{
		return MultipleOf ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="OneOfKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[count]] - the number of subschemas that passed validation
	/// </remarks>
	public static string? OneOf { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="OneOfKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[count]] - the number of subschemas that passed validation
	/// </remarks>
	public static string GetOneOf(CultureInfo? culture)
	{
		return OneOf ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="PatternKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[pattern]] - the regular expression
	/// </remarks>
	public static string? Pattern { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="PatternKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[pattern]] - the regular expression
	/// </remarks>
	public static string GetPattern(CultureInfo? culture)
	{
		return Pattern ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="RequiredKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the properties missing from the JSON instance
	/// </remarks>
	public static string? Required { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="RequiredKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[missing]] - the properties missing from the JSON instance
	/// </remarks>
	public static string GetRequired(CultureInfo? culture)
	{
		return Required ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for when the schema cannot be validated
	/// against the meta-schema.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[uri]] - the URI of the meta-schema
	/// </remarks>
	public static string? MetaSchemaValidation { get; set; }

	/// <summary>
	/// Gets or sets the error message for when the schema cannot be validated
	/// against the meta-schema.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[uri]] - the URI of the meta-schema
	/// </remarks>
	public static string GetMetaSchemaValidation(CultureInfo? culture)
	{
		return MetaSchemaValidation ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="TypeKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the type of value provided in the JSON instance
	///   - [[expected]] - the type(s) required by the schema
	/// </remarks>
	public static string? Type { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="TypeKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the type of value provided in the JSON instance
	///   - [[expected]] - the type(s) required by the schema
	/// </remarks>
	public static string GetType(CultureInfo? culture)
	{
		return Type ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="UniqueItemsKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[duplicates]] - the indices of duplicate pairs as a comma-delimited list of "(x, y)" items
	/// </remarks>
	public static string? UniqueItems { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="UniqueItemsKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[duplicates]] - the indices of duplicate pairs as a comma-delimited list of "(x, y)" items
	/// </remarks>
	public static string GetUniqueItems(CultureInfo? culture)
	{
		return UniqueItems ?? Get(culture);
	}
}