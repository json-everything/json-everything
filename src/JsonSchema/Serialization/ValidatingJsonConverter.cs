using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
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
	/// Default options factory used when creating validating converters.
	/// Creates a new JsonSerializerOptions without ValidatingJsonConverter instances to avoid recursion.
	/// </summary>
	public static readonly Func<JsonSerializerOptions, JsonSerializerOptions> DefaultOptionsFactory = o =>
	{
		var newOptions = new JsonSerializerOptions(o);
		var existingConverters = o.Converters.OfType<ValidatingJsonConverter>().ToArray();
		foreach (var c in existingConverters)
		{
			newOptions.Converters.Remove(c);
		}
		return newOptions;
	};

	/// <summary>
	/// Provides evaluation options for the validator.
	/// </summary>
	public EvaluationOptions EvaluationOptions { get; set; } = new();

	/// <summary>
	/// Adds an explicit type/schema mapping for types external types which cannot be decorated with <see cref="JsonSchemaAttribute"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="schema"></param>
	public static void MapType<T>(JsonSchema schema)
	{
		_instance.CreateConverter(typeof(T), schema);
	}

	/// <summary>
	/// Registers a pre-created converter for a specific type.
	/// Used primarily by source-generated code to register converters for AOT scenarios.
	/// </summary>
	/// <param name="type">The type to register the converter for.</param>
	/// <param name="converter">The converter instance to register.</param>
	public static void RegisterConverter(Type type, JsonConverter converter)
	{
		_instance.SetOptions(converter);
		_cache[type] = converter;
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

		var schema = GetSchema(typeToConvert);
		return CreateConverter(typeToConvert, schema);
	}

	/// <summary>
	/// Gets the schema for a type.
	/// </summary>
	protected virtual JsonSchema GetSchema(Type type)
	{
		var schemaAttribute = (JsonSchemaAttribute)type.GetCustomAttributes(typeof(JsonSchemaAttribute)).Single();
		return schemaAttribute.Schema;
	}

	private JsonConverter CreateConverter(Type typeToConvert, JsonSchema schema)
	{
		var converterType = typeof(ValidatingJsonConverter<>).MakeGenericType(typeToConvert);

		var converter = (JsonConverter)Activator.CreateInstance(converterType, schema, DefaultOptionsFactory)!;

		SetOptions(converter);
		_cache[typeToConvert] = converter;

		return converter;
	}

	private void SetOptions(JsonConverter converter)
	{
		var validatingConverter = (IValidatingJsonConverter)converter;
		validatingConverter.Options = EvaluationOptions;
	}
}

internal interface IValidatingJsonConverter
{
	EvaluationOptions? Options { get; set; }
}

/// <summary>
/// DO NOT USE THIS CLASS DIRECTLY.  INSTEAD, REGISTER THE NON-GENERIC CONVERTER WITH YOUR SERIALIZER OPTIONS.
/// Validates JSON against a schema during deserialization.
/// </summary>
/// <typeparam name="T">The type being converted.</typeparam>
public class ValidatingJsonConverter<T> : WeaklyTypedJsonConverter<T>, IValidatingJsonConverter
{
	private readonly JsonSchema _schema;
	private readonly Func<JsonSerializerOptions, JsonSerializerOptions> _optionsFactory;

	/// <summary>
	/// Gets or sets the evaluation options for schema validation.
	/// </summary>
	public EvaluationOptions? Options { get; set; }

	/// <summary>
	/// Creates a new instance of <see cref="ValidatingJsonConverter{T}"/>.
	/// </summary>
	/// <param name="schema">The JSON schema to validate against.</param>
	/// <param name="optionsFactory">A factory function to create serializer options without validating converters to avoid recursion.</param>
	public ValidatingJsonConverter(JsonSchema schema, Func<JsonSerializerOptions, JsonSerializerOptions> optionsFactory)
	{
		_schema = schema;
		_optionsFactory = optionsFactory;
	}

	/// <summary>
	/// Reads and validates JSON data against the schema.
	/// </summary>
	/// <param name="reader">The reader to use for reading JSON.</param>
	/// <param name="typeToConvert">The type to convert to.</param>
	/// <param name="options">The serializer options.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="JsonException">Thrown when the JSON does not meet schema requirements.</exception>
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var readerCopy = reader;
		var node = options.Read(ref readerCopy, JsonSchemaSerializerContext.Default.JsonElement);

		var validation = _schema.Evaluate(node, Options);

		var newOptions = _optionsFactory(options);

		if (validation.IsValid)
			// TODO: this does not use the options reader because at least one test failed with a NotSupportedException
			/*
			return newOptions.Read<T>(ref reader);
			/*/
			return JsonSerializer.Deserialize<T>(ref reader, newOptions);
		//*/

		throw new JsonException("JSON does not meet schema requirements")
		{
			Data =
			{
				["validation"] = validation
			}
		};
	}

	/// <summary>Reads a dictionary key from a JSON property name.</summary>
	/// <param name="reader">The <see cref="T:System.Text.Json.Utf8JsonReader" /> to read from.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">The options to use when reading the value.</param>
	/// <returns>The value that was converted.</returns>
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var newOptions = _optionsFactory(options);
		return JsonSerializer.Deserialize<T>($"\"{reader.GetString()}\"", newOptions)!;
	}

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The writer to use for writing JSON.</param>
	/// <param name="value">The value to serialize.</param>
	/// <param name="options">The serializer options.</param>
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		var newOptions = _optionsFactory(options);

		newOptions.Write(writer, value, typeof(T));
	}

	/// <summary>Writes a dictionary key as a JSON property name.</summary>
	/// <param name="writer">The <see cref="T:System.Text.Json.Utf8JsonWriter" /> to write to.</param>
	/// <param name="value">The value to convert. The value of <see cref="P:System.Text.Json.Serialization.JsonConverter`1.HandleNull" /> determines if the converter handles <see langword="null" /> values.</param>
	/// <param name="options">The options to use when writing the value.</param>
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] T value, JsonSerializerOptions options)
	{
		var newOptions = _optionsFactory(options);
		var serialized = JsonSerializer.SerializeToNode(value, newOptions);
		if (serialized is not JsonValue jsonValue || !jsonValue.TryGetValue(out string? key))
			throw new JsonException($"The value {value} cannot be used as a JSON key because it does not serialize to a string.");

		writer.WritePropertyName(key);
		
	}
}