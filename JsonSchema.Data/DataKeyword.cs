using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

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
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "data";

	/// <summary>
	/// Gets or sets a method to download external references.
	/// </summary>
	/// <remarks>
	/// The default method simply attempts to download the resource.  There is no
	/// caching involved.
	/// </remarks>
	public static Func<Uri, JsonNode?>? Fetch { get; set; }

	/// <summary>
	/// Provides a registry for known external data sources.
	/// </summary>
	/// <remarks>
	/// This property stores full JSON documents retrievable by URI.  If the desired
	/// value exists as a sub-value of a document, a JSON Pointer URI fragment identifier
	/// should be used in the `data` keyword do identify the exact value location.
	/// </remarks>
	public static ConcurrentDictionary<Uri, JsonValue?> ExternalDataRegistry { get; } = new();


	/// <summary>
	/// The collection of keywords and references.
	/// </summary>
	public IReadOnlyDictionary<string, IDataResourceIdentifier> References { get; }

	/// <summary>
	/// Creates an instance of the <see cref="DataKeyword"/> class.
	/// </summary>
	/// <param name="references">The collection of keywords and references.</param>
	public DataKeyword(IReadOnlyDictionary<string, IDataResourceIdentifier> references)
	{
		References = references;
	}

	/// <summary>
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	/// <exception cref="JsonException">
	/// Thrown when the formed schema contains values that are invalid for the associated
	/// keywords.
	/// </exception>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		var data = new Dictionary<string, JsonNode>();
		var failedReferences = new List<IDataResourceIdentifier>();
		foreach (var reference in References)
		{
			if (!reference.Value.TryResolve(context, out var resolved))
				failedReferences.Add(reference.Value);

			data.Add(reference.Key, resolved!);
		}

		if (failedReferences.Any())
			throw new RefResolutionException(failedReferences.Select(x => x.ToString()));

		var json = JsonSerializer.Serialize(data);
		var subschema = JsonSerializer.Deserialize<JsonSchema>(json)!;

		context.Push(context.EvaluationPath.Combine(Name), subschema);
		context.Validate();
		var result = context.LocalResult.IsValid;
		context.Pop();
		if (!result)
			context.LocalResult.Fail();
		context.ExitKeyword(Name);
	}

	/// <summary>
	/// Provides a simple data fetch method that supports `http`, `https`, and `file` URI schemes.
	/// </summary>
	/// <param name="uri">The URI to fetch.</param>
	/// <returns>A JSON string representing the data</returns>
	/// <exception cref="FormatException">
	/// Thrown when the URI scheme is not `http`, `https`, or `file`.
	/// </exception>
	public static JsonNode? SimpleDownload(Uri uri)
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
				throw new FormatException($"URI scheme '{uri.Scheme}' is not supported.  Only HTTP(S) and local file system URIs are allowed.");
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
			.ToDictionary(kvp => kvp.Key, kvp => JsonSchemaBuilderExtensions.CreateResourceIdentifier(kvp.Value));
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