using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.CodeGeneration.Model;

public class ObjectModel : TypeModel
{
	public PropertyModel[] Properties { get; }

	public ObjectModel(string name, IEnumerable<PropertyModel> properties)
	{
		Name = name;
		Properties = properties.ToArray();
	}
}