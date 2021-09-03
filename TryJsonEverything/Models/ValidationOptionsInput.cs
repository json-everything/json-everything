using System;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema;

namespace TryJsonEverything.Models
{
	public class ValidationOptionsInput
	{ 
		[JsonConverter(typeof(EnumStringConverter<OutputFormat>))]
		public OutputFormat? OutputFormat { get; set; }
		[JsonConverter(typeof(SchemaDraftConverter))]
		public Draft? ValidateAs { get; set; }
		public Uri? DefaultBaseUri { get; set; }
		public bool? RequireFormatValidation { get; set; }

		public ValidationOptions ToValidationOptions()
		{
			return new ValidationOptions
			{
				OutputFormat = OutputFormat ?? ValidationOptions.Default.OutputFormat,
				ValidateAs = ValidateAs ?? ValidationOptions.Default.ValidateAs,
				DefaultBaseUri = DefaultBaseUri ?? ValidationOptions.Default.DefaultBaseUri,
				RequireFormatValidation = RequireFormatValidation ?? ValidationOptions.Default.RequireFormatValidation
			};
		}
	}
}