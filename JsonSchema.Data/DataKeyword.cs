using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema.Data
{
	[SchemaKeyword(Name)]
	[SchemaPriority(int.MinValue)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.DataId)]
	[JsonConverter(typeof(DataKeywordJsonConverter))]
	public class DataKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "data";
		internal static readonly Regex AnchorPattern = new Regex("^[A-Za-z][-A-Za-z0-9.:_]*$");

		private static Func<Uri, string> _get;

		public static Func<Uri, string> Get
		{
			get => _get;
			set => _get = value ?? SimpleDownload;
		}

		public IReadOnlyDictionary<string, Uri> References { get; }

		public DataKeyword(IReadOnlyDictionary<string, Uri> references)
		{
			References = references;
		}

		public void Validate(ValidationContext context)
		{
			var data = new Dictionary<string, JsonElement>();
			foreach (var reference in References)
			{
				var resolved = Resolve(context, reference.Value);
				if (resolved == null) return;

				data.Add(reference.Key, resolved.Value);
			}
				
			var json = JsonSerializer.Serialize(data);
			var subschema = JsonSerializer.Deserialize<JsonSchema>(json);
			subschema.ValidateSubschema(context);
		}

		private static JsonElement? Resolve(ValidationContext context, Uri target)
		{
			var parts = target.OriginalString.Split(new[] { '#' }, StringSplitOptions.None);
			var baseUri = parts[0];
			var fragment = parts.Length > 1 ? parts[1] : null;

			JsonElement data = default;
			if (!string.IsNullOrEmpty(baseUri))
			{
				if (Uri.TryCreate(baseUri, UriKind.Absolute, out var newUri))
					data = Download(newUri);
				else if (context.CurrentUri != null)
				{
					var uriFolder = context.CurrentUri.OriginalString.EndsWith("/")
						? context.CurrentUri
						: context.CurrentUri.GetParentUri();
					var newBaseUri = new Uri(uriFolder, baseUri);
					data = Download(newBaseUri);
				}
			}
			else
				data = context.InstanceRoot;

			if (Equals(data, default(JsonElement)))
			{
				context.IsValid = false;
				context.Message = $"Could not resolve base URI `{baseUri}`";
				return null;
			}

			if (!string.IsNullOrEmpty(fragment))
			{
				fragment = $"#{fragment}";
				if (!JsonPointer.TryParse(fragment, out var pointer))
				{
					context.IsValid = false;
					context.Message = $"Could not parse pointer `{fragment}`";
					return null;
				}

				var resolved = pointer.Evaluate(data);
				if (resolved == null)
				{
					context.IsValid = false;
					context.Message = $"Could not resolve pointer `{fragment}`";
					return null;
				}
				data = resolved.Value;
			}

			if (Equals(data, default(JsonElement)))
			{
				context.IsValid = false;
				context.Message = $"Could not resolve URI `{baseUri}`";
				return null;
			}

			return data;
		}

		private static JsonElement Download(Uri uri)
		{
			var data = Get(uri);
			return JsonDocument.Parse(data).RootElement;
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
	}

	internal class DataKeywordJsonConverter : JsonConverter<DataKeyword>
	{
		public override DataKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var references = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options)
				.ToDictionary(kvp => kvp.Key, kvp => new Uri(kvp.Value));
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
}
