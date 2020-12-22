using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Logic.Components
{
	[Operator("missing")]
	internal class MissingComponent : LogicComponent
	{
		private readonly LogicComponent[] _components;

		public MissingComponent(params LogicComponent[] components)
		{
			_components = components;
		}

		public override JsonElement Apply(JsonElement data)
		{
			var expected = _components.SelectMany(c => c.Apply(data).Flatten())
				.Where(e => e.ValueKind == JsonValueKind.String);

			if (data.ValueKind != JsonValueKind.Object)
				return expected.AsJsonElement();

			var paths = expected.Select(e => e.GetString())
				.Select(p => new {Path = p, Pointer = JsonPointer.Parse(p == string.Empty ? "" : $"/{p.Replace('.', '/')}")})
				.Select(p => new {Path = p.Path, Value = p.Pointer.Evaluate(data)});

			return paths.Where(p => p.Value == null)
				.Select(k => k.Path.AsJsonElement())
				.AsJsonElement();

		}
	}
}