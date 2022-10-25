using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.More.Tests;

internal class GithubTests
{
	[Test]
	[Ignore("JsonElement.GetString() isn't thread safe.  See https://github.com/dotnet/runtime/issues/77421#issuecomment-1290198540")]
	public void Issue337_ParallelComparisons()
	{
		string? failed = null;

		var arrayText = "[\"DENIED\",\"GRANTED\"]";
		var valueText = "\"GRANTED\"";

		var array = JsonNode.Parse(arrayText);
		var value = JsonNode.Parse(valueText);

		try
		{
			Parallel.ForEach(Enumerable.Range(1, 1000000).ToList().AsParallel(), i =>
			{
				var array0 = array[0];
				var array1 = array[1];
				var array0Hash = JsonNodeEqualityComparer.Instance.GetHashCode(array0);
				var array1Hash = JsonNodeEqualityComparer.Instance.GetHashCode(array1);
				var valueHash = JsonNodeEqualityComparer.Instance.GetHashCode(value);

				if (array0Hash != valueHash && array1Hash != valueHash)
				{
					failed ??= $@"Hashcode failed on iteration {i}

value: {valueHash} - {value.AsJsonString()}
array[0]: {array0Hash} - {array0.AsJsonString()}
array[1]: {array1Hash} - {array1.AsJsonString()}";
					return;
				}

				//if (!JsonNodeEqualityComparer.Instance.Equals(array0, value) &&
				//	!JsonNodeEqualityComparer.Instance.Equals(array1, value))
				//{
				//	failed ??= "Equals failed";
				//}

				//if (!array.Contains(value, JsonNodeEqualityComparer.Instance))
				//{
				//	failed ??= "Contains failed";
				//}
			});
		}
		finally
		{
			if (failed != null)
			{
				Console.WriteLine(failed);
				Assert.Fail();
			}
		}
	}
	[Test]
	public void Issue337_ParallelComparisonsButWithNumbers()
	{
		string? failed = null;

		var arrayText = "[1,2]";
		var valueText = "2";

		var array = JsonNode.Parse(arrayText);
		var value = JsonNode.Parse(valueText);

		try
		{
			Parallel.ForEach(Enumerable.Range(1, 1000000).ToList().AsParallel(), i =>
			{
				var array0 = array[0];
				var array1 = array[1];
				var array0Hash = JsonNodeEqualityComparer.Instance.GetHashCode(array0);
				var array1Hash = JsonNodeEqualityComparer.Instance.GetHashCode(array1);
				var valueHash = JsonNodeEqualityComparer.Instance.GetHashCode(value);

				if (array0Hash != valueHash && array1Hash != valueHash)
				{
					failed ??= $@"Hashcode failed on iteration {i}

value: {valueHash} - {value.AsJsonString()}
array[0]: {array0Hash} - {array0.AsJsonString()}
array[1]: {array1Hash} - {array1.AsJsonString()}";
					return;
				}

				//if (!JsonNodeEqualityComparer.Instance.Equals(array0, value) &&
				//	!JsonNodeEqualityComparer.Instance.Equals(array1, value))
				//{
				//	failed ??= "Equals failed";
				//}

				//if (!array.Contains(value, JsonNodeEqualityComparer.Instance))
				//{
				//	failed ??= "Contains failed";
				//}
			});
		}
		finally
		{
			if (failed != null)
			{
				Console.WriteLine(failed);
				Assert.Fail();
			}
		}
	}
}