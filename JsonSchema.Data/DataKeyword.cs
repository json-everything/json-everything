using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.Data;

/// <summary>
/// Represents the `data` keyword.
/// </summary>
[SchemaKeyword(Name)]
[SchemaPriority(int.MinValue)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.DataId)]
[JsonConverter(typeof(DataKeywordJsonConverter))]
public class DataKeyword : IJsonSchemaKeyword, IEquatable<DataKeyword>
{
	internal const string Name = "data";

	private static Func<Uri, string>? _get;

	/// <summary>
	/// Gets or sets a method to download external references.
	/// </summary>
	/// <remarks>
	/// The default method simply attempts to download the resource.  There is no
	/// caching involved.
	/// </remarks>
	public static Func<Uri, string> Get
	{
		get => _get ??= SimpleDownload;
		set => _get = value;
	}

	/// <summary>
	/// The collection of keywords and references.
	/// </summary>
	public IReadOnlyDictionary<string, Uri> References { get; }

	/// <summary>
	/// Creates an instance of the <see cref="DataKeyword"/> class.
	/// </summary>
	/// <param name="references">The collection of keywords and references.</param>
	public DataKeyword(IReadOnlyDictionary<string, Uri> references)
	{
		References = references;
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		var data = new Dictionary<string, JsonNode>();
		foreach (var reference in References)
		{
			if (!TryResolve(context, reference.Value, out var resolved)) return;

			data.Add(reference.Key, resolved!);
		}

		var json = JsonSerializer.Serialize(data);
		JsonSchema subschema;
		try
		{
			subschema = JsonSerializer.Deserialize<JsonSchema>(json)!;
		}
		catch (JsonException e)
		{
			context.LocalResult.Fail(Name, e.Message);
			return;
		}

		subschema.ValidateSubschema(context);
		context.ExitKeyword(Name);
	}

	private static bool TryResolve(ValidationContext context, Uri target, out JsonNode? node)
	{
		var parts = target.OriginalString.Split(new[] { '#' }, StringSplitOptions.None);
		var baseUri = parts[0];
		var fragment = parts.Length > 1 ? parts[1] : null;

		JsonNode? data;
		if (!string.IsNullOrEmpty(baseUri))
		{
			bool wasResolved;
			if (Uri.TryCreate(baseUri, UriKind.Absolute, out var newUri))
				wasResolved = TryDownload(newUri, out data);
			else
			{
				var uriFolder = context.CurrentUri.OriginalString.EndsWith("/")
					? context.CurrentUri
					: context.CurrentUri.GetParentUri();
				var newBaseUri = new Uri(uriFolder, baseUri);
				wasResolved = TryDownload(newBaseUri, out data);
			}

			if (!wasResolved)
			{
				context.LocalResult.Fail(Name, ErrorMessages.BaseUriResolution, ("uri", baseUri));
				node = null;
				return false;
			}
		}
		else
			data = context.InstanceRoot;

		if (!string.IsNullOrEmpty(fragment))
		{
			fragment = $"#{fragment}";
			if (!JsonPointer.TryParse(fragment, out var pointer))
			{
				context.LocalResult.Fail(Name, ErrorMessages.PointerParse, ("fragment", fragment));
				node = null;
				return false;
			}

			if (!pointer!.TryEvaluate(data, out var resolved))
			{
				context.LocalResult.Fail(Name, ErrorMessages.RefResolution, ("uri", fragment));
				node = null;
				return false;
			}
			data = resolved;
		}

		node = data;
		return true;
	}

	private static bool TryDownload(Uri uri, out JsonNode? node)
	{
		var data = Get(uri);
		if (data == null)
		{
			node = null;
			return false;
		}
		node = JsonNode.Parse(data);
		return true;
	}

	private static string SimpleDownload(Uri uri)
	{
		switch (uri.Scheme)
		{
			case "http":
			case "https":
				return new HttpClient().GetStringAsync(uri).Result;
			case "file":
				var filename = Uri.UnescapeDataString(uri.AbsolutePath);
				return File.ReadAllText(filename);
			default:
				throw new Exception($"URI scheme '{uri.Scheme}' is not supported.  Only HTTP(S) and local file system URIs are allowed.");
		}
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(DataKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (References.Count != other.References.Count) return false;
		var byKey = References.Join(other.References,
				td => td.Key,
				od => od.Key,
				(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
			.ToList();
		if (byKey.Count != References.Count) return false;

		return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as DataKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return References.GetHashCode();
	}
}

internal class DataKeywordJsonConverter : JsonConverter<DataKeyword>
{
	public override DataKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var references = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options)!
			.ToDictionary(kvp => kvp.Key, kvp => new Uri(kvp.Value, UriKind.RelativeOrAbsolute));
		return new DataKeyword(references);
	}

	public override void Write(Utf8JsonWriter writer, DataKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DataKeyword.Name);
		writer.WriteStartObject();
		foreach (var kvp in value.References)
		{
			writer.WritePropertyName(kvp.Key);
			JsonSerializer.Serialize(writer, kvp.Value, options);
		}
		writer.WriteEndObject();
	}
}