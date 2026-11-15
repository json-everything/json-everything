using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Serialization;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration;

public static class DataAnnotationsTestModels
{
	[GenerateJsonSchema]
	public class PersonWithMaxLength
	{
		[System.ComponentModel.DataAnnotations.MaxLength(50)]
		public string Name { get; set; } = string.Empty;
		
		public int Age { get; set; }
	}

	[GenerateJsonSchema]
	public class PersonWithMinLength
	{
		[System.ComponentModel.DataAnnotations.MinLength(2)]
		public string Name { get; set; } = string.Empty;
		
		public int Age { get; set; }
	}

	[GenerateJsonSchema]
	public class PersonWithStringLength
	{
		[StringLength(50, MinimumLength = 2)]
		public string Name { get; set; } = string.Empty;
		
		public int Age { get; set; }
	}

	[GenerateJsonSchema]
	public class PersonWithRange
	{
		public string Name { get; set; } = string.Empty;
		
		[Range(0, 120)]
		public int Age { get; set; }
	}

	[GenerateJsonSchema]
	public class PersonWithRegex
	{
		[RegularExpression(@"^\d{3}-\d{2}-\d{4}$")]
		public string SSN { get; set; } = string.Empty;
	}

	[GenerateJsonSchema]
	public class PersonWithEmail
	{
		[EmailAddress]
		public string Email { get; set; } = string.Empty;
	}

	[GenerateJsonSchema]
	public class PersonWithUrl
	{
		[Url]
		public string Website { get; set; } = string.Empty;
	}

#if NET8_0_OR_GREATER
	[GenerateJsonSchema]
	public class PersonWithLength
	{
		[Length(2, 50)]
		public string Name { get; set; } = string.Empty;
	}

	[GenerateJsonSchema]
	public class PersonWithBase64
	{
		[Base64String]
		public string Image { get; set; } = string.Empty;
	}
#endif

	[GenerateJsonSchema]
	public class PersonWithMultipleConstraints
	{
		[StringLength(50, MinimumLength = 2)]
		[RegularExpression(@"^[A-Za-z\s]+$")]
		public string Name { get; set; } = string.Empty;
		
		[Range(18, 120)]
		public int Age { get; set; }
		
		[EmailAddress]
		public string Email { get; set; } = string.Empty;
	}
}
