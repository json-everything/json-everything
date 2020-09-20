namespace Json.Path
{
	public class PathIndex
	{
		public string Name { get; }
		public int? Index { get; }

		private PathIndex(string name)
		{
			Name = name;
		}

		private PathIndex(int index)
		{
			Index = index;
		}

		public static implicit operator PathIndex(string name)
		{
			return new PathIndex(name);
		}

		public static implicit operator PathIndex(int index)
		{
			return new PathIndex(index);
		}
	}
}