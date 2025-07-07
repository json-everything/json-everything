﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.Serialization;

/// <summary>
/// Adds schema validation for types decorated with the <see cref="JsonSchemaAttribute"/>.
/// </summary>
[RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.")]
public class ValidatingJsonConverter : JsonConverterFactory
{
	private static readonly ConcurrentDictionary<Type, JsonConverter?> _cache = new();
	private static readonly ValidatingJsonConverter _instance = new();

	/// <summary>
	/// Specifies the output format.
	/// </summary>
	public OutputFormat? OutputFormat { get; set; }
	/// <summary>
	/// Specifies whether the `format` keyword should be required to provide
	/// validation results.  Default is false, which just produces annotations
	/// for drafts 2019-09 and prior or follows the behavior set forth by the
	/// format-annotation vocabulary requirement in the `$vocabulary` keyword in
	/// a meta-schema declaring draft 2020-12.
	/// </summary>
	public bool? RequireFormatValidation { get; set; }

	/// <summary>
	/// Adds an explicit type/schema mapping for types external types which cannot be decorated with <see cref="JsonSchemaAttribute"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="schema"></param>
	public static void MapType<T>(JsonSchemaNode schema)
	{
		_instance.CreateConverter(typeof(T), schema);
	}

	/// <summary>When overridden in a derived class, determines whether the converter instance can convert the specified object type.</summary>
	/// <param name="typeToConvert">The type of the object to check whether it can be converted by this converter instance.</param>
	/// <returns>
	/// <see langword="true" /> if the instance can convert the specified object type; otherwise, <see langword="false" />.</returns>
	public override bool CanConvert(Type typeToConvert)
	{
		if (_cache.TryGetValue(typeToConvert, out var converter)) return converter != null;

		var canConvert = typeToConvert.GetCustomAttributes(typeof(JsonSchemaAttribute)).SingleOrDefault() != null;
		if (!canConvert)
			_cache[typeToConvert] = null;

		return canConvert;
	}

	/// <summary>Creates a converter for a specified type.</summary>
	/// <param name="typeToConvert">The type handled by the converter.</param>
	/// <param name="options">The serialization options to use.</param>
	/// <returns>
	/// An instance of a <see cref="JsonConverter{T}"/> where `T` is compatible with <paramref name="typeToConvert"/>.
	/// If <see langword="null"/> is returned, a <see cref="NotSupportedException"/> will be thrown.
	/// </returns>
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		// at this point, we know that we should have a converter, so we don't need to check for null
		if (_cache.TryGetValue(typeToConvert, out var converter))
		{
			// override its options using this converter's options
			SetOptions(converter!);
			return converter;
		}

		var schemaAttribute = (JsonSchemaAttribute)typeToConvert.GetCustomAttributes(typeof(JsonSchemaAttribute)).Single();
		var schema = schemaAttribute.Schema;

		return CreateConverter(typeToConvert, schema);
	}

	private JsonConverter CreateConverter(Type typeToConvert, JsonSchemaNode schema)
	{
		var converterType = typeof(ValidatingJsonConverter<>).MakeGenericType(typeToConvert);

		// ReSharper disable once ConvertToLocalFunction
		Func<JsonSerializerOptions, JsonSerializerOptions> optionsFactory = o =>
		{
			var newOptions = new JsonSerializerOptions(o);
			var existingConverters = o.Converters.OfType<ValidatingJsonConverter>().ToArray();
			foreach (var c in existingConverters)
			{
				newOptions.Converters.Remove(c);
			}
			return newOptions;
		};
		var converter = (JsonConverter)Activator.CreateInstance(converterType, schema, optionsFactory)!;

		SetOptions(converter);

		_cache[typeToConvert] = converter;

		return converter;
	}

	private void SetOptions(JsonConverter converter)
	{
		var validatingConverter = (IValidatingJsonConverter)converter;
		validatingConverter.OutputFormat = OutputFormat ?? Schema.OutputFormat.Flag;
		validatingConverter.RequireFormatValidation = RequireFormatValidation ?? false;
	}
}

internal interface IValidatingJsonConverter
{
	public OutputFormat OutputFormat { get; set; }
	public bool RequireFormatValidation { get; set; }
}

internal class ValidatingJsonConverter<T> : WeaklyTypedJsonConverter<T>, IValidatingJsonConverter
{
	private readonly JsonSchemaNode _schema;
	private readonly Func<JsonSerializerOptions, JsonSerializerOptions> _optionsFactory;

	public OutputFormat OutputFormat { get; set; } = OutputFormat.Flag;
	public bool RequireFormatValidation { get; set; }

	public ValidatingJsonConverter(JsonSchemaNode schema, Func<JsonSerializerOptions, JsonSerializerOptions> optionsFactory)
	{
		_schema = schema;
		_optionsFactory = optionsFactory;
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		//var readerCopy = reader;
		//var node = options.Read(ref readerCopy, JsonSchemaSerializerContext.Default.JsonNode);
		
		//var validation = _schema.Evaluate(node, new EvaluationOptions
		//{
		//	OutputFormat = OutputFormat,
		//	RequireFormatValidation = RequireFormatValidation
		//});

		//var newOptions = _optionsFactory(options);

		//if (validation.IsValid)
		//	// TODO: this does not use the options reader because at least one test failed with a NotSupportedException
		//	/*
		//	return newOptions.Read<T>(ref reader);
		//	/*/
		//	return JsonSerializer.Deserialize<T>(ref reader, newOptions);
		//	//*/

		//throw new JsonException("JSON does not meet schema requirements")
		//{
		//	Data =
		//	{
		//		["validation"] = validation
		//	}
		//};

		throw new NotImplementedException();
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		var newOptions = _optionsFactory(options);

		newOptions.Write(writer, value, typeof(T));
	}
}