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
}