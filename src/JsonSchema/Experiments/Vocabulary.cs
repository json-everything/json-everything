using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class Vocabulary
{
	public Uri Id { get; }
	public JsonObject MetaSchema { get; }
	public IKeywordHandler[] Handlers { get; }

	public Vocabulary(Uri id, JsonObject metaSchema, IEnumerable<IKeywordHandler> handlers)
	{
		Id = id;
		MetaSchema = metaSchema;
		Handlers = [.. handlers];
	}

	public Vocabulary(Uri id, JsonObject metaSchema, params IKeywordHandler[] handlers)
	{
		Id = id;
		MetaSchema = metaSchema;
		Handlers = handlers;
	}
}