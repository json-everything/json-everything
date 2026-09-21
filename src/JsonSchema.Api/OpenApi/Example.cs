using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models an example.
/// </summary>
[JsonConverter(typeof(ExampleJsonConverter))]
public class Example : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	public string? Summary { get; set; }
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or sets the example value.
	/// </summary>
	public JsonNode? Value { get; set; }
	/// <summary>
	/// Gets or sets a URI that points to the literal example.
	/// </summary>
	public string? ExternalValue { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		if (keys[0] == "value")
		{
			if (keys.Length == 1) return Value;
			keys[1..].ToPointer().TryEvaluate(Value, out var target);
			return target;
		}

		return ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		if (this is ExampleRef exRef)
			yield return exRef;
	}
}

/// <summary>
/// Models a `$ref` to an example.
/// </summary>
public class ExampleRef : Example, IComponentRef
{
	/// <summary>
	/// The URI for the reference.
	/// </summary>
	public Uri Ref { get; }

	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	public new string? Summary { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public new string? Description { get; set; }

	/// <summary>
	/// Gets whether the reference has been resolved.
	/// </summary>
	public bool IsResolved { get; private set; }

	/// <summary>
	/// Creates a new <see cref="ExampleRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public ExampleRef(Uri reference)
	{
		Ref = reference ?? throw new ArgumentNullException(nameof(reference));
	}

	/// <summary>
	/// Creates a new <see cref="ExampleRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public ExampleRef(string reference)
	{
		Ref = new Uri(reference ?? throw new ArgumentNullException(nameof(reference)), UriKind.RelativeOrAbsolute);
	}

	async Task IComponentRef.Resolve(OpenApiDocument root, JsonSerializerOptions? options)
	{
		void copy(Example other)
		{
			base.Summary = other.Summary;
			base.Description = other.Description;
			Value = other.Value;
			ExternalValue = other.ExternalValue;
			ExtensionData = other.ExtensionData;
		}

		IsResolved = await OpenApi.Ref.Resolve(root, Ref, copy, ApiSerializerContext.Default.Example, options);
	}
}

internal class ExampleJsonConverter : JsonConverter<Example>
{
	private const string _objectType = "example";

	public override Example Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? reference = null;
		string? summary = null;
		string? description = null;
		JsonNode? value = null;
		string? externalValue = null;
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
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				case "value":
					value = reader.ReadNode();
					break;
				case "externalValue":
					externalValue = reader.ReadString(propertyName, _objectType);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		if (reference is not null)
			return new ExampleRef(reference)
			{
				Summary = summary,
				Description = description
			};

		return new Example
		{
			Summary = summary,
			Description = description,
			Value = value,
			ExternalValue = externalValue,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, Example value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value is ExampleRef reference)
		{
			writer.WriteString("$ref", reference.Ref.ToString());
			writer.MaybeWrite("summary", reference.Summary);
			writer.MaybeWrite("description", reference.Description);
		}
		else
		{
			writer.MaybeWrite("summary", value.Summary);
			writer.MaybeWrite("description", value.Description);
			writer.MaybeWriteNode("value", value.Value);
			writer.MaybeWrite("externalValue", value.ExternalValue);
			writer.WriteExtensions(value.ExtensionData, value.UnknownData);
		}

		writer.WriteEndObject();
	}
}
