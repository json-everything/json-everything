using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema
{
	public class Vocabulary
	{
		public Uri Id { get; }
		public IReadOnlyCollection<Type> Keywords { get; }

		public Vocabulary(string id, params Type[] keywords)
		{
			Id = new Uri(id, UriKind.Absolute);
			Keywords = keywords.ToList();
		}

		public Vocabulary(string id, IEnumerable<Type> keywords)
		{
			Id = new Uri(id, UriKind.Absolute);
			Keywords = keywords.ToList();
		}
	}
}