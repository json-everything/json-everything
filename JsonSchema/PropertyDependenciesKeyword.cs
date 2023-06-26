using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles the `propertyDependencies` keyword.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(PropertyDependenciesKeywordJsonConverter))]
public class PropertyDependenciesKeyword : IJsonSchemaKeyword, ICustomSchemaCollector, IEquatable<PropertyDependenciesKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "propertyDependencies";

	/// <summary>
	/// Gets the collection of dependencies.
	/// </summary>
	public IReadOnlyDictionary<string, PropertyDependency> Dependencies { get; }

	IEnumerable<JsonSchema> ICustomSchemaCollector.Schemas => Dependencies.SelectMany(x => x.Value.Schemas.Select(y => y.Value));
	/// <summary>
	/// Creates a new instance of the <see cref="PropertyDependenciesKeyword"/>.
	/// </summary>
	/// <param name="dependencies">The collection of dependencies.</param>
	public PropertyDependenciesKeyword(IReadOnlyDictionary<string, PropertyDependency> dependencies)
	{
		Dependencies = dependencies;
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
		var overallResult = true;

		var tasks = Dependencies.Select(async property =>
		{
			if (tokenSource.Token.IsCancellationRequested) return ((string?)null, (bool?)null);

			context.Log(() => $"Evaluating property '{property.Key}'.");
			var dependency = property.Value;
			var name = property.Key;
			if (!obj.TryGetPropertyValue(name, out var value))
			{
				context.Log(() => $"Property '{property.Key}' does not exist. Skipping.");
				return (null, null);
			}

			if (value.GetSchemaValueType() != SchemaValueType.String)
			{
				context.Log(() => $"Property '{property.Key}' is not a string. Skipping.");
				return (null, null);
			}

			var stringValue = value!.GetValue<string>();
			if (!dependency.Schemas.TryGetValue(stringValue, out var schema))
			{
				context.Log(() => $"Property '{property.Key}' does not specify a requirement for value '{stringValue}'");
				return (null, null);
			}

			var branch = context.ParallelBranch(context.EvaluationPath.Combine(name, stringValue), schema);
			await branch.Evaluate(tokenSource.Token);
			context.Log(() => $"Property '{property.Key}' {branch.LocalResult.IsValid.GetValidityString()}.");

			return (property.Key, branch.LocalResult.IsValid);
		}).ToArray();

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

		if (!overallResult)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	(JsonSchema?, int) ICustomSchemaCollector.FindSubschema(IReadOnlyList<PointerSegment> segments)
	{
		if (segments.Count < 2) return (null, 0);
		if (!Dependencies.TryGetValue(segments[0].Value, out var property)) return (null, 0);
		if (!property.Schemas.TryGetValue(segments[1].Value, out var schema)) return (null, 0);

		return (schema, 2);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(PropertyDependenciesKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Dependencies.Count != other.Dependencies.Count) return false;
		var byKey = Dependencies.Join(other.Dependencies,
				td => td.Key,
				od => od.Key,
				(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
			.ToArray();
		if (byKey.Length != Dependencies.Count) return false;

		return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as PropertyDependenciesKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Dependencies.GetStringDictionaryHashCode();
	}
}

internal class PropertyDependenciesKeywordJsonConverter : JsonConverter<PropertyDependenciesKeyword>
{
	public override PropertyDependenciesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var dependencies = JsonSerializer.Deserialize<Dictionary<string, PropertyDependency>>(ref reader, options);

		return new PropertyDependenciesKeyword(dependencies!);
	}

	public override void Write(Utf8JsonWriter writer, PropertyDependenciesKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(PropertyDependenciesKeyword.Name);
		JsonSerializer.Serialize(writer, value.Dependencies, options);
	}
}