using System;

namespace Json.Schema
{
	/// <summary>
	/// Indicates that the keyword is classified as an applicator.
	/// </summary>
	/// <remarks>
	/// Apply this attribute to your schema keyword if it contains subschemas
	/// that also provide validations.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ApplicatorAttribute : Attribute
	{
	}
}