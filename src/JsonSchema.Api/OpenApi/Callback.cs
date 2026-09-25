using System;
using Json.More;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a callback.
/// </summary>
[JsonConverter(typeof(CallbackJsonConverter))]
public class Callback : Dictionary<CallbackKeyExpression, PathItem>, IRefTargetContainer
{
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
		if (keys.Length == 0) return null;

		return this.GetFromMap(keys[0])?.Resolve(keys.Slice(1)) ??
		       ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		return Values.SelectMany(x => x.FindSchemas());
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		if (this is CallbackRef cRef)
			yield return cRef;

		var theRest = Values.SelectMany(x => x.FindRefs());

		foreach (var reference in theRest)
		{
			yield return reference;
		}
	}
}

/// <summary>
/// Models a `$ref` to a callback.
/// </summary>
public class CallbackRef : Callback, IComponentRef
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
	public string? Description { get; set; }

	/// <summary>
	/// Gets whether the reference has been resolved.
	/// </summary>
	public bool IsResolved { get; private set; }

	/// <summary>
	/// Creates a new <see cref="CallbackRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public CallbackRef(Uri reference)
	{
		Ref = reference ?? throw new ArgumentNullException(nameof(reference));
	}

	/// <summary>
	/// Creates a new <see cref="CallbackRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public CallbackRef(string reference)
	{
		Ref = new Uri(reference ?? throw new ArgumentNullException(nameof(reference)), UriKind.RelativeOrAbsolute);
	}

	async Task IComponentRef.Resolve(OpenApiDocument root, JsonSerializerOptions? options)
	{
		void copy(Callback other)
		{
			ExtensionData = other.ExtensionData;
			foreach (var kvp in other)
			{
				this[kvp.Key] = kvp.Value;
			}
		}

		IsResolved = await OpenApi.Ref.Resolve(root, Ref, copy, ApiSerializerContext.Default.Callback, options);
	}
}

internal class CallbackJsonConverter : JsonConverter<Callback>
{
	private const string _objectType = "callback";

	public override Callback Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? reference = null;
		string? summary = null;
		string? description = null;
		var callback = new Callback();
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
				default:
					if (ConverterHelpers.IsExtensionKey(propertyName))
					{
						reader.ReadExtension(propertyName, ref extensionData);
						break;
					}

					callback.Add(
						CallbackKeyExpression.Parse(propertyName),
						options.Read(ref reader, ApiSerializerContext.Default.PathItem)!);
					break;
			}
		}

		if (reference is not null)
			return new CallbackRef(reference)
			{
				Summary = summary,
				Description = description
			};

		callback.ExtensionData = extensionData;
		callback.UnknownData = unknownData;

		return callback;
	}

	public override void Write(Utf8JsonWriter writer, Callback value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value is CallbackRef reference)
		{
			writer.WriteString("$ref", reference.Ref.ToString());
			writer.MaybeWrite("summary", reference.Summary);
			writer.MaybeWrite("description", reference.Description);
		}
		else
		{
			foreach (var kvp in value)
			{
				writer.WritePropertyName(kvp.Key.ToString());
				options.Write(writer, kvp.Value, ApiSerializerContext.Default.PathItem);
			}

			writer.WriteExtensions(value.ExtensionData, value.UnknownData);
		}

		writer.WriteEndObject();
	}
}