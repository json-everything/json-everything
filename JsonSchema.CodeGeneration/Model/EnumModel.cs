using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.CodeGeneration.Model;

public class EnumModel : TypeModel
{
	public EnumValue[] Values { get; }

	public EnumModel(string name, IEnumerable<(string name, int value)> values)
		: base(name)
	{
		Values = values.Select(x => new EnumValue(x.name, x.value)).ToArray();
	}

	public EnumModel(string name, IEnumerable<string> names)
		: base(name)
	{
		Values = names.Select((n, i) => new EnumValue(n, i)).ToArray();
	}

	public override bool Equals(TypeModel? other)
	{
		if (!base.Equals(other)) return false;

		var enumModel = (EnumModel)other;
		return Values.SequenceEqual(enumModel.Values);
	}
}