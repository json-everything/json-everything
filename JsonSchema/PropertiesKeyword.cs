using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Json.Schema;

/// <summary>
/// Handles `properties`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(PropertiesKeywordJsonConverter))]
public class PropertiesKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector, IEquatable<PropertiesKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "properties";

	/// <summary>
	/// The property schemas.
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> Properties { get; }

	IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Properties;

	/// <summary>
	/// Creates a new <see cref="PropertiesKeyword"/>.
	/// </summary>
	/// <param name="values">The property schemas.</param>
	public PropertiesKeyword(IReadOnlyDictionary<string, JsonSchema> values)
	{
		Properties = values ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	/// <param name="token">The cancellation token used by the caller.</param>
	public async Task Evaluate(EvaluationContext context, CancellationToken token)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.Object)
		{
			context.WrongValueKind(schemaValueType);
			return;
		}

		var obj = (JsonObject)context.LocalInstance!;
		if (!obj.VerifyJsonObject()) return;

		var tokenSource = new CancellationTokenSource();
		token.Register(tokenSource.Cancel);

		context.Options.LogIndentLevel++;
		bool overallResult = true;
		var evaluatedProperties = new List<string>();

		var tasks = Properties.Select(property => Task.Run(async () =>
		{
			if (tokenSource.Token.IsCancellationRequested) return ((string?)null, (bool?)null);

			context.Log(() => $"Evaluating property '{property.Key}'.");
			var schema = property.Value;
			var name = property.Key;
			if (!obj.TryGetPropertyValue(name, out var item))
			{
				context.Log(() => $"Property '{property.Key}' does not exist. Skipping.");
				return (null, null);
			}

			var branch = context.ParallelBranch(context.InstanceLocation.Combine(name),
				item,
				context.EvaluationPath.Combine(Name, name),
				schema);
			await branch.Evaluate(tokenSource.Token);
			var localResult = branch.LocalResult.IsValid;
			context.Log(() => $"Property '{property.Key}' {localResult.GetValidityString()}.");

			return (name, localResult);
		}, tokenSource.Token)).ToArray();

		if (tasks.Any())
		{
			if (context.ApplyOptimizations)
			{
				var failedValidation = await tasks.WhenAny(x => !x.Item2 ?? false, tokenSource.Token);
				tokenSource.Cancel();

				overallResult = failedValidation == null;
			}
			else
			{
				await Task.WhenAll(tasks);
				overallResult = tasks.All(x => x.Result.Item2 ?? true);
			}
		}

		context.Options.LogIndentLevel--;
		evaluatedProperties.AddRange(tasks.Select(x => x.Result.Item1!).Where(x => x != null));
		context.LocalResult.SetAnnotation(Name, JsonSerializer.SerializeToNode(evaluatedProperties));

		if (!overallResult)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(PropertiesKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Properties.Count != other.Properties.Count) return false;
		var byKey = Properties.Join(other.Properties,
				td => td.Key,
				od => od.Key,
				(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
			.ToArray();
		if (byKey.Length != Properties.Count) return false;

		return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as PropertiesKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Properties.GetStringDictionaryHashCode();
	}
}

internal class PropertiesKeywordJsonConverter : JsonConverter<PropertiesKeyword>
{
	public override PropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options)!;
		return new PropertiesKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, PropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(PropertiesKeyword.Name);
		writer.WriteStartObject();
		foreach (var kvp in value.Properties)
		{
			writer.WritePropertyName(kvp.Key);
			JsonSerializer.Serialize(writer, kvp.Value, options);
		}
		writer.WriteEndObject();
	}
}