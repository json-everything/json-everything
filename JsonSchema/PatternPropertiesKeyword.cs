using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `patternProperties`.
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
[JsonConverter(typeof(PatternPropertiesKeywordJsonConverter))]
public class PatternPropertiesKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector, IEquatable<PatternPropertiesKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "patternProperties";

	/// <summary>
	/// The pattern-keyed schemas.
	/// </summary>
	public IReadOnlyDictionary<Regex, JsonSchema> Patterns { get; }
	/// <summary>
	/// If any pattern is invalid or unsupported by <see cref="Regex"/>, it will appear here.
	/// </summary>
	/// <remarks>
	/// All validations will fail if this is populated.
	/// </remarks>
	public IReadOnlyList<string>? InvalidPatterns { get; }

	IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Patterns.ToDictionary(x => x.Key.ToString(), x => x.Value);

	/// <summary>
	/// Creates a new <see cref="PatternPropertiesKeyword"/>.
	/// </summary>
	/// <param name="values">The pattern-keyed schemas.</param>
	public PatternPropertiesKeyword(IReadOnlyDictionary<Regex, JsonSchema> values)
	{
		Patterns = values ?? throw new ArgumentNullException(nameof(values));
	}
	internal PatternPropertiesKeyword(IReadOnlyDictionary<Regex, JsonSchema> values, IReadOnlyList<string> invalidPatterns)
	{
		Patterns = values ?? throw new ArgumentNullException(nameof(values));
		InvalidPatterns = invalidPatterns;
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
		context.Options.LogIndentLevel++;
		var overallResult = true;
		var evaluatedProperties = new List<string>();

		var tokenSource = new CancellationTokenSource();
		token.Register(tokenSource.Cancel);

		var tasks = Patterns.SelectMany(p => obj.Where(kvp => p.Key.IsMatch(kvp.Key))
			.Select(async instanceProperty =>
			{
				if (tokenSource.Token.IsCancellationRequested) return ((string?)null, (bool?)null);

				context.Log(() => $"Validating property '{instanceProperty.Key}'.");
				var branch = context.ParallelBranch(context.InstanceLocation.Combine(instanceProperty.Key),
					instanceProperty.Value ?? JsonNull.SignalNode,
					context.EvaluationPath.Combine(Name, PointerSegment.Create($"{p.Key}")),
					p.Value);
				await branch.Evaluate(tokenSource.Token);
				context.Log(() => $"Property '{instanceProperty.Key}' {branch.LocalResult.IsValid.GetValidityString()}.");

				return (instanceProperty.Key, branch.LocalResult.IsValid);
			}
		)).ToArray();

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

		if (InvalidPatterns?.Any() ?? false)
		{
			foreach (var pattern in InvalidPatterns)
			{
				context.Push(context.EvaluationPath.Combine(pattern), false);
				context.LocalResult.Fail(Name, ErrorMessages.InvalidPattern, ("pattern", pattern));
				overallResult = false;
				context.Log(() => $"Discovered invalid pattern '{pattern}'.");
				context.Pop();
				if (!overallResult && context.ApplyOptimizations) break;
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
	public bool Equals(PatternPropertiesKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Patterns.Count != other.Patterns.Count) return false;
		var byKey = Patterns.Join(other.Patterns,
				td => td.Key.ToString(),
				od => od.Key.ToString(),
				(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
			.ToArray();
		if (byKey.Length != Patterns.Count) return false;

		return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as PatternPropertiesKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return ((IKeyedSchemaCollector)this).Schemas.GetStringDictionaryHashCode();
	}
}

internal class PatternPropertiesKeywordJsonConverter : JsonConverter<PatternPropertiesKeyword>
{
	public override PatternPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var patternProps = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options)!;
		var schemas = new Dictionary<Regex, JsonSchema>();
		var invalidProps = new List<string>();
		foreach (var prop in patternProps)
		{
			try
			{
				var regex = new Regex(prop.Key, RegexOptions.ECMAScript | RegexOptions.Compiled);
				schemas.Add(regex, prop.Value);
			}
			catch
			{
				invalidProps.Add(prop.Key);
			}
		}
		return new PatternPropertiesKeyword(schemas, invalidProps);
	}
	public override void Write(Utf8JsonWriter writer, PatternPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(PatternPropertiesKeyword.Name);
		writer.WriteStartObject();
		foreach (var schema in value.Patterns)
		{
			writer.WritePropertyName(schema.Key.ToString());
			JsonSerializer.Serialize(writer, schema.Value, options);
		}
		writer.WriteEndObject();
	}
}