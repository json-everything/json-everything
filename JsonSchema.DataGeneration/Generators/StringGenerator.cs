using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Bogus;
using Bogus.Extensions;
using Json.More;

namespace Json.Schema.DataGeneration.Generators
{
	internal class StringGenerator : IDataGenerator
	{
		private const int _maxStringLength = 1000;
		private static readonly NumberRangeSet _defaultRange = NumberRangeSet.NonNegative.Floor(0).Ceiling(_maxStringLength);
		private static readonly Faker _faker = new Faker();
		private static readonly Dictionary<string, Func<NumberRange, JsonElementProxy>> _formatGenerators =
			new Dictionary<string, Func<NumberRange, JsonElementProxy>>
			{
				[Formats.Date.Key] = GenerateDate,
				[Formats.DateTime.Key] = GenerateDateTime,
				[Formats.Duration.Key] = GenerateDuration,
				[Formats.Email.Key] = GenerateEmail,
				[Formats.Hostname.Key] = GenerateHostname,
				[Formats.IdnEmail.Key] = GenerateEmail,
				[Formats.IdnHostname.Key] = GenerateHostname,
				[Formats.Ipv4.Key] = GenerateIpv4,
				[Formats.Ipv6.Key] = GenerateIpv6,
				[Formats.Iri.Key] = GenerateUri,
				[Formats.IriReference.Key] = GenerateUriReference,
				[Formats.JsonPointer.Key] = GenerateJsonPointer,
				[Formats.RelativeJsonPointer.Key] = GenerateRelativeJsonPointer,
				[Formats.Time.Key] = GenerateTime,
				[Formats.Uri.Key] = GenerateUri,
				[Formats.UriReference.Key] = GenerateUriReference,
				[Formats.Uuid.Key] = GenerateUuid
			};

		// TODO: move these to a public settings object
		public static uint DefaultMinLength { get; set; } = 0;
		public static uint DefaultMaxLength { get; set; } = 100;

		public static StringGenerator Instance { get; } = new StringGenerator();

		private StringGenerator()
		{
		}

		public SchemaValueType Type => SchemaValueType.String;

		public GenerationResult Generate(RequirementsContext context)
		{
			var ranges = context.StringLengths ?? _defaultRange;
			var range = JsonSchemaExtensions.Randomizer.ArrayElement(ranges.Ranges.ToArray());
			var minimum = range.Minimum.Value != NumberRangeSet.MinRangeValue
				? (uint) Math.Max(0, (long) range.Minimum.Value)
				: Math.Max(0, DefaultMinLength);
			var maximum = range.Maximum.Value != NumberRangeSet.MaxRangeValue
				? (uint) Math.Min(_maxStringLength, range.Maximum.Value)
				: Math.Min(_maxStringLength, DefaultMaxLength);

			//var rangeRegex = $".{{{range.Minimum.Value},{range.Maximum.Value}}}";
			
			//string overallRegex = string.Empty;

			//if (context.Patterns != null)
			//{
			//	if (context.Patterns.Count == 1)
			//		overallRegex = context.Patterns[0].ToString();
			//	else
			//		overallRegex += HelperExtensions.Require(context.Patterns.Select(x => x.ToString()));
			//}

			//if (context.AntiPatterns != null)
			//	overallRegex += HelperExtensions.Forbid(context.AntiPatterns.Select(x => x.ToString()));

			//overallRegex += rangeRegex;
			//overallRegex = $"^{overallRegex}$";

			//Console.WriteLine(overallRegex);

			if (context.Format != null)
			{
				if (!_formatGenerators.TryGetValue(context.Format, out var generate))
					return GenerationResult.Fail($"Cannot generate data for format '{context.Format}'");

				return GenerationResult.Success(generate(range));
			}

			var data = _faker.Lorem.Text().ClampLength((int) minimum, (int) maximum);
			return GenerationResult.Success(data);
		}

		private static JsonElementProxy GenerateDate(NumberRange numberRange)
		{
			return _faker.Date.Recent().ToString("yyyy-MM-dd");
		}

		private static JsonElementProxy GenerateDateTime(NumberRange numberRange)
		{
			return _faker.Date.Recent().ToString("O");
		}

		private static JsonElementProxy GenerateDuration(NumberRange arg)
		{
			var days = JsonSchemaExtensions.Randomizer.Int(0, 6);
			var hours = JsonSchemaExtensions.Randomizer.Int(0, 23);
			var minutes = JsonSchemaExtensions.Randomizer.Int(0, 59);
			var seconds = JsonSchemaExtensions.Randomizer.Int(0, 59);

			return $"P{days}DT{hours}H{minutes}M{seconds}S";
		}

		private static JsonElementProxy GenerateEmail(NumberRange arg)
		{
			return _faker.Person.Email;
		}

		private static JsonElementProxy GenerateHostname(NumberRange arg)
		{
			return _faker.Internet.DomainName();
		}

		private static JsonElementProxy GenerateIpv4(NumberRange arg)
		{
			return _faker.Internet.Ip();
		}

		private static JsonElementProxy GenerateIpv6(NumberRange arg)
		{
			return _faker.Internet.Ipv6();
		}

		private static JsonElementProxy GenerateJsonPointer(NumberRange arg)
		{
			var segmentCount = JsonSchemaExtensions.Randomizer.Int(0, 10);
			var segments = Enumerable.Range(0, segmentCount)
				.Select(x => JsonSchemaExtensions.Randomizer.UInt(0, 2) == 0
					? JsonSchemaExtensions.Randomizer.UInt(0, 10).ToString()
					: _faker.Lorem.Word());

			return $"/{string.Join("/", segments)}";
		}

		private static JsonElementProxy GenerateRelativeJsonPointer(NumberRange arg)
		{
			var countUp = JsonSchemaExtensions.Randomizer.Int(0, 7);
			JsonElement pointer = GenerateJsonPointer(arg);
			return $"{countUp}{pointer.GetString()}";
		}

		private static JsonElementProxy GenerateTime(NumberRange arg)
		{
			return _faker.Date.Recent().ToString("HH':'mm':'ss");
		}

		private static JsonElementProxy GenerateUri(NumberRange arg)
		{
			return _faker.Internet.UrlWithPath();
		}

		private static JsonElementProxy GenerateUriReference(NumberRange arg)
		{
			return _faker.Internet.UrlWithPath();
		}

		private static JsonElementProxy GenerateUuid(NumberRange arg)
		{
			return Guid.NewGuid().ToString("D");
		}
	}
}