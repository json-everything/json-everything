using NUnit.Framework;
// ReSharper disable NonReadonlyMemberInGetHashCode
#pragma warning disable NUnit2005
#pragma warning disable CS8618

namespace Yaml2JsonNode.Tests;

public class SerializerTests
{
	private class Bar
	{
		public string OtherString { get; set; }

		protected bool Equals(Bar other)
		{
			return OtherString == other.OtherString;
		}

		public override bool Equals(object? obj)
		{
			if (obj is null) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Bar)obj);
		}

		public override int GetHashCode()
		{
			return OtherString.GetHashCode();
		}
	}

	private class Foo
	{
		public string StringProp { get; set; }
		public int IntProp { get; set; }
		public decimal DecimalProp { get; set; }
		public double DoubleProp { get; set; }
		public bool BoolProp { get; set; }
		public List<int> ListOfInts { get; set; }
		public Dictionary<string, string> MapOfStrings { get; set; }
		public Bar NestedObject { get; set; }

		protected bool Equals(Foo other)
		{
			var listEqual = ListOfInts.Zip(other.ListOfInts)
				.All(x => x.First == x.Second);
			var joinedMap = MapOfStrings.Join(other.MapOfStrings,
				x => x.Key,
				y => y.Key,
				(x, y) => x.Value == y.Value)
				.ToList();
			var mapEqual = joinedMap.Count == MapOfStrings.Count && joinedMap.All(x => x);

			return StringProp == other.StringProp &&
			       IntProp == other.IntProp &&
			       DecimalProp == other.DecimalProp &&
			       DoubleProp.Equals(other.DoubleProp) &&
			       BoolProp == other.BoolProp &&
			       listEqual &&
			       mapEqual &&
			       NestedObject.Equals(other.NestedObject);
		}

		public override bool Equals(object? obj)
		{
			if (obj is null) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Foo)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(StringProp, IntProp, DecimalProp, DoubleProp, BoolProp, ListOfInts, MapOfStrings, NestedObject);
		}
	}

	[Test]
	public void SerializeObject()
	{
		var foo = new Foo
		{
			StringProp = "string",
			IntProp = 42,
			DecimalProp = 42.5m,
			DoubleProp = 42.9,
			BoolProp = true,
			ListOfInts = new List<int> { 1, 2, 3, 4 },
			MapOfStrings = new Dictionary<string, string>
			{
				["string1"] = "found",
				["string2"] = "lost"
			},
			NestedObject = new Bar
			{
				OtherString = "yep"
			}
		};

		var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "expected-serialized.yaml")
			.AdjustForPlatform();

		var expected = File.ReadAllText(path);

		var actual = YamlSerializer.Serialize(foo);

		Assert.AreEqual(expected, actual);
	}

	[Test]
	public void DeserializeObject()
	{
		var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "expected-serialized.yaml")
			.AdjustForPlatform();

		var text = File.ReadAllText(path);

		var expected = new Foo
		{
			StringProp = "string",
			IntProp = 42,
			DecimalProp = 42.5m,
			DoubleProp = 42.9,
			BoolProp = true,
			ListOfInts = new List<int> { 1, 2, 3, 4 },
			MapOfStrings = new Dictionary<string, string>
			{
				["string1"] = "found",
				["string2"] = "lost"
			},
			NestedObject = new Bar
			{
				OtherString = "yep"
			}
		};


		var actual = YamlSerializer.Deserialize<Foo>(text);

		Assert.AreEqual(expected, actual);
	}
}