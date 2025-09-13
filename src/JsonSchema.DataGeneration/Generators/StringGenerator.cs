using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Bogus;
using Bogus.Extensions;
using Fare;

namespace Json.Schema.DataGeneration.Generators;

internal class StringGenerator : IDataGenerator
{
	private const int _maxStringLength = 1000;
	private static readonly NumberRangeSet _defaultRange = NumberRangeSet.NonNegative.Floor(0).Ceiling(_maxStringLength);
	private static readonly Faker _faker = new();
	private static readonly Dictionary<string, Func<NumberRange, JsonNode?>> _formatGenerators =
		new()
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

	public static StringGenerator Instance { get; } = new();

	private StringGenerator()
	{
	}

	public SchemaValueType Type => SchemaValueType.String;

	public GenerationResult Generate(RequirementsContext context)
	{
		if (context.Pattern != null)
		{
			if (context.StringLengths != null) throw new NotSupportedException("Cannot generate strings that match regex and also specify string lengths");

			string overallRegex = string.Empty;

			//if (context.Patterns != null)
			//{
			//	if (context.Patterns.Count == 1)
			//		overallRegex = context.Patterns[0].ToString();
			//	else
			//		overallRegex += HelperExtensions.Require(context.Patterns.Select(x => x.ToString()));
			//}

			//if (context.AntiPatterns != null)
			//	overallRegex += HelperExtensions.Forbid(context.AntiPatterns.Select(x => x.ToString()));
			if (context.Pattern != null)
				overallRegex = context.Pattern;

#if DEBUG
			Console.WriteLine(overallRegex);
#endif
			return GenerationResult.Success(new Xeger(overallRegex).Generate());
		}

		var ranges = context.StringLengths ?? _defaultRange;
		var range = JsonSchemaExtensions.Randomizer.ArrayElement(ranges.Ranges.ToArray());
		var minimum = range.Minimum.Value != NumberRangeSet.MinRangeValue
			? (uint)Math.Max(0, (long)range.Minimum.Value)
			: Math.Max(0, DefaultMinLength);
		var maximum = range.Maximum.Value != NumberRangeSet.MaxRangeValue
			? (uint)Math.Min(_maxStringLength, range.Maximum.Value)
			: Math.Min(_maxStringLength, DefaultMaxLength);

		if (context.Format != null)
		{
			if (!_formatGenerators.TryGetValue(context.Format, out var generate))
				return GenerationResult.Fail($"Cannot generate data for format '{context.Format}'");

			return GenerationResult.Success(generate(range));
		}

		var data = _faker.Lorem.Text().ClampLength((int)minimum, (int)maximum);
		return GenerationResult.Success(data);
	}

	private static JsonNode? GenerateDate(NumberRange numberRange)
	{
		return _faker.Date.Recent().ToString("yyyy-MM-dd");
	}

	private static JsonNode? GenerateDateTime(NumberRange numberRange)
	{
		return _faker.Date.Recent().ToString("O");
	}

	private static JsonNode? GenerateDuration(NumberRange arg)
	{
		var days = JsonSchemaExtensions.Randomizer.Int(0, 6);
		var hours = JsonSchemaExtensions.Randomizer.Int(0, 23);
		var minutes = JsonSchemaExtensions.Randomizer.Int(0, 59);
		var seconds = JsonSchemaExtensions.Randomizer.Int(0, 59);

		return $"P{days}DT{hours}H{minutes}M{seconds}S";
	}

	private static JsonNode? GenerateEmail(NumberRange arg)
	{
		return _faker.Person.Email;
	}

	private static JsonNode? GenerateHostname(NumberRange arg)
	{
		return _faker.Internet.DomainName();
	}

	private static JsonNode? GenerateIpv4(NumberRange arg)
	{
		return _faker.Internet.Ip();
	}

	private static JsonNode? GenerateIpv6(NumberRange arg)
	{
		return _faker.Internet.Ipv6();
	}

	private static JsonNode? GenerateJsonPointer(NumberRange arg)
	{
		var segmentCount = JsonSchemaExtensions.Randomizer.Int(0, 10);
		var segments = Enumerable.Range(0, segmentCount)
			.Select(_ => JsonSchemaExtensions.Randomizer.UInt(0, 2) == 0
				? JsonSchemaExtensions.Randomizer.UInt(0, 10).ToString()
				: _faker.Lorem.Word());

		return $"/{string.Join("/", segments)}";
	}

	private static JsonNode? GenerateRelativeJsonPointer(NumberRange arg)
	{
		var countUp = JsonSchemaExtensions.Randomizer.Int(0, 7);
		var pointer = GenerateJsonPointer(arg);
		return $"{countUp}{pointer!.GetValue<string>()}";
	}

	private static JsonNode? GenerateTime(NumberRange arg)
	{
		return _faker.Date.Recent().ToString("HH':'mm':'ss");
	}

	private static JsonNode? GenerateUri(NumberRange arg)
	{
		return _faker.Internet.UrlWithPath();
	}

	private static JsonNode? GenerateUriReference(NumberRange arg)
	{
		return _faker.Internet.UrlWithPath();
	}

	private static JsonNode? GenerateUuid(NumberRange arg)
	{
		return Guid.NewGuid().ToString("D");
	}
}