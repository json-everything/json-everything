using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class IndexSegment : IContextAccessorSegment
{
	private readonly int _index;

	public IndexSegment(int index)
	{
		_index = index;
	}

	public bool TryFind(JsonNode? target, out JsonNode? value)
	{
		if (target is JsonArray arr)
		{
			if (_index < 0)
			{
				if (-_index < arr.Count)
				{
					value = arr[arr.Count + _index];
					return true;
				}
			}
			else if (_index < arr.Count)
			{
				value = arr[_index];
				return true;
			}
		}

		value = null;
		return false;
	}
}