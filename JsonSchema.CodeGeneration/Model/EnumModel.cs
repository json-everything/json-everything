using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.CodeGeneration.Model;

public class EnumModel : TypeModel
{
	public EnumValue[] Values { get; }

	public EnumModel(string name, IEnumerable<(string name, int value)> values)
	{
		Name = name;
		Values = values.Select(x => new EnumValue(x.name, x.value)).ToArray();
	}

	public EnumModel(string name, IEnumerable<string> names)
	{
		Name = name;
		Values = names.Select((n, i) => new EnumValue(n, i)).ToArray();
	}
}