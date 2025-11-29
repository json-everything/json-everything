using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

public partial class Vocabulary
{
	public Uri Id { get; }
	
	public IKeywordHandler[] Keywords { get; }

	public Vocabulary(Uri id, params IKeywordHandler[] keywords)
	{
		Id = id;
		Keywords = keywords;
	}

	public Vocabulary(Uri id, IEnumerable<IKeywordHandler> keywords)
	{
		Id = id;
		Keywords = keywords.ToArray();
	}
}