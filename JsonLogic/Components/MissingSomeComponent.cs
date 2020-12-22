using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Logic.Components
{
	[Operator("missing_some")]
	internal class MissingSomeComponent : LogicComponent
	{
		private readonly LogicComponent _requiredCount;
		private readonly LogicComponent _components;

		public MissingSomeComponent(LogicComponent requiredCount, LogicComponent components)
		{
			_requiredCount = requiredCount;
			_components = components;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var requiredCount = _requiredCount.Apply(data).Numberify();
			var components = _components.Apply(data);
			if (components.ValueKind != JsonValueKind.Array)
				throw new JsonLogicException("Expected array of required paths.");
			
			var expected = components.EnumerateArray().SelectMany(e => e.Flatten()).ToList();
			if (expected.Any(e => e.ValueKind != JsonValueKind.String))
				throw new JsonLogicException("Expected array of required paths.");

			if (data.ValueKind != JsonValueKind.Object)
				return expected.AsJsonElement();

			var paths = expected.Select(e => e.GetString())
				.Select(p => new { Path = p, Pointer = JsonPointer.Parse(p == string.Empty ? "" : $"/{p.Replace('.', '/')}") })
				.Select(p => new { Path = p.Path, Value = p.Pointer.Evaluate(data) });

			var missing = paths.Where(p => p.Value == null)
				.Select(k => k.Path.AsJsonElement())
				.ToList();
			var found = paths.Count(p => p.Value != null);

			if (found < requiredCount)
				return missing.AsJsonElement();

			return new JsonElement[0].AsJsonElement();
		}
	}
}