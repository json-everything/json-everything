using System;
using Json.More;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a parameter.
/// </summary>
[JsonConverter(typeof(ParameterJsonConverter))]
public class Parameter : IRefTargetContainer
{
	/// <summary>
	/// Gets the name.
	/// </summary>
	public string Name { get; private protected set; }
	/// <summary>
	/// Gets the parameter location.
	/// </summary>
	public ParameterLocation In { get; private protected set; }
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or sets whether the parameter is required.
	/// </summary>
	public bool? Required { get; set; }
	/// <summary>
	/// Gets or sets whether the parameter is deprecated.
	/// </summary>
	public bool? Deprecated { get; set; }
	/// <summary>
	/// Gets or sets whether the parameter is allowed to be present with an empty value.
	/// </summary>
	public bool? AllowEmptyValue { get; set; }
	/// <summary>
	/// Gets or sets how the parameter value will be serialized.
	/// </summary>
	public ParameterStyle? Style { get; set; }
	/// <summary>
	/// Gets or sets whether this will be exploded into multiple parameters.
	/// </summary>
	public bool? Explode { get; set; }
	/// <summary>
	/// Gets or sets whether the parameter value should allow reserved characters.
	/// </summary>
	public bool? AllowReserved { get; set; }
	/// <summary>
	/// Gets or sets a schema for the content.
	/// </summary>
	public JsonSchema? Schema { get; set; }
	/// <summary>
	/// Gets or sets an example.
	/// </summary>
	public JsonNode? Example { get; set; }
	/// <summary>
	/// Gets or sets a collection of examples.
	/// </summary>
	public Dictionary<string, Example>? Examples { get; set; }
	/// <summary>
	/// Gets or sets a collection of content.
	/// </summary>
	public Dictionary<string, MediaType>? Content { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="Parameter"/>
	/// </summary>
	/// <param name="name">The name</param>
	/// <param name="in">The parameter location</param>
	public Parameter(string name, ParameterLocation @in)
	{
		Name = name;
		In = @in;
	}
#pragma warning disable CS8618
	private protected Parameter(){}
#pragma warning restore CS8618

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		int keysConsumed = 1;
		IRefTargetContainer? target = null;
		switch (keys[0])
		{
			case "schema":
				if (Schema == null) return null;
				if (keys.Length == 1) return Schema;
				// TODO: consider some other kind of value being buried in a schema
				throw new NotImplementedException();
			case "example":
				return Example?.GetFromNode(keys.Slice(1));
			case "examples":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Examples?.GetFromMap(keys[1]);
				break;
			case "content":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Content?.GetFromMap(keys[1]);
				break;
		}

		return target != null
			? target.Resolve(keys.Slice(keysConsumed))
			: ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		if (Schema != null)
			yield return Schema;

		var theRest = GeneralHelpers.Collect(Content?.Values.SelectMany(x => x.FindSchemas()));

		foreach (var schema in theRest)
		{
			yield return schema;
		}
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		if (this is ParameterRef pRef)
			yield return pRef;

		var theRest = GeneralHelpers.Collect(
			Examples?.Values.SelectMany(x => x.FindRefs())
		);

		foreach (var reference in theRest)
		{
			yield return reference;
		}
	}
}

/// <summary>
/// Models a `$ref` to a parameter.
/// </summary>
public class ParameterRef : Parameter, IComponentRef
{
	/// <summary>
	/// The URI for the reference.
	/// </summary>
	public Uri Ref { get; }

	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	public string? Summary { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public new string? Description { get; set; }

	/// <summary>
	/// Gets whether the reference has been resolved.
	/// </summary>
	public bool IsResolved { get; private set; }

	/// <summary>
	/// Creates a new <see cref="ParameterRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public ParameterRef(Uri reference)
	{
		Ref = reference ?? throw new ArgumentNullException(nameof(reference));
	}

	/// <summary>
	/// Creates a new <see cref="ParameterRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public ParameterRef(string reference)
	{
		Ref = new Uri(reference ?? throw new ArgumentNullException(nameof(reference)), UriKind.RelativeOrAbsolute);
	}

	async Task IComponentRef.Resolve(OpenApiDocument root, JsonSerializerOptions? options)
	{
		void copy(Parameter other)
		{
			Name = other.Name;
			In = other.In;
			base.Description = other.Description;
			Required = other.Required;
			Deprecated = other.Deprecated;
			AllowEmptyValue = other.AllowEmptyValue;
			Style = other.Style;
			Explode = other.Explode;
			AllowReserved = other.AllowReserved;
			Schema = other.Schema;
			Example = other.Example;
			Examples = other.Examples;
			Content = other.Content;
			ExtensionData = other.ExtensionData;
		}

		IsResolved = await OpenApi.Ref.Resolve(root, Ref, copy, ApiSerializerContext.Default.Parameter, options);
	}
}

internal class ParameterJsonConverter : JsonConverter<Parameter>
{
	private const string _objectType = "parameter";

	public override Parameter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? reference = null;
		string? summary = null;
		string? name = null;
		ParameterLocation? location = null;
		string? description = null;
		bool? required = null;
		bool? deprecated = null;
		bool? allowEmptyValue = null;
		ParameterStyle? style = null;
		bool? explode = null;
		bool? allowReserved = null;
		JsonSchema? schema = null;
		JsonNode? example = null;
		Dictionary<string, Example>? examples = null;
		Dictionary<string, MediaType>? content = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "$ref":
					reference = reader.ReadUri(propertyName, _objectType);
					break;
				case "summary":
					summary = reader.ReadString(propertyName, _objectType);
					break;
				case "name":
					name = reader.ReadString(propertyName, _objectType);
					break;
				case "in":
					location = reader.ReadEnum(propertyName, _objectType, options, ApiSerializerContext.Default.ParameterLocation);
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				case "required":
					required = reader.ReadBool(propertyName, _objectType);
					break;
				case "deprecated":
					deprecated = reader.ReadBool(propertyName, _objectType);
					break;
				case "allowEmptyValue":
					allowEmptyValue = reader.ReadBool(propertyName, _objectType);
					break;
				case "style":
					style = reader.ReadEnum(propertyName, _objectType, options, ApiSerializerContext.Default.ParameterStyle);
					break;
				case "explode":
					explode = reader.ReadBool(propertyName, _objectType);
					break;
				case "allowReserved":
					allowReserved = reader.ReadBool(propertyName, _objectType);
					break;
				case "schema":
					schema = options.Read(ref reader, ApiSerializerContext.Default.JsonSchema);
					break;
				case "example":
					example = reader.ReadNode();
					break;
				case "examples":
					examples = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Example);
					break;
				case "content":
					content = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.MediaType);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		if (reference is not null)
			return new ParameterRef(reference)
			{
				Summary = summary,
				Description = description
			};

		if (location is null)
			throw new JsonException($"`in` is required for {_objectType}.");

		return new Parameter(ConverterHelpers.ExpectPresent(name, "name", _objectType), location.Value)
		{
			Description = description,
			Required = required,
			Deprecated = deprecated,
			AllowEmptyValue = allowEmptyValue,
			Style = style,
			Explode = explode,
			AllowReserved = allowReserved,
			Schema = schema,
			Example = example,
			Examples = examples,
			Content = content,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, Parameter value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value is ParameterRef reference)
		{
			writer.WriteString("$ref", reference.Ref.ToString());
			writer.MaybeWrite("summary", reference.Summary);
			writer.MaybeWrite("description", reference.Description);
		}
		else
		{
			writer.WriteString("name", value.Name);
			writer.WritePropertyName("in");
			options.Write(writer, value.In, ApiSerializerContext.Default.ParameterLocation);
			writer.MaybeWrite("description", value.Description);
			writer.MaybeWrite("required", value.Required);
			writer.MaybeWrite("deprecated", value.Deprecated);
			writer.MaybeWrite("allowEmptyValue", value.AllowEmptyValue);
			writer.MaybeWriteEnum("style", value.Style, options, ApiSerializerContext.Default.ParameterStyle);
			writer.MaybeWrite("explode", value.Explode);
			writer.MaybeWrite("allowReserved", value.AllowReserved);
			writer.MaybeWrite("schema", value.Schema, options, ApiSerializerContext.Default.JsonSchema);
			writer.MaybeWriteNode("example", value.Example);
			writer.MaybeWriteMap("examples", value.Examples, options, ApiSerializerContext.Default.Example);
			writer.MaybeWriteMap("content", value.Content, options, ApiSerializerContext.Default.MediaType);
			writer.WriteExtensions(value.ExtensionData, value.UnknownData);
		}

		writer.WriteEndObject();
	}
}