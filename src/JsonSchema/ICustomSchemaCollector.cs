﻿using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Indicates that the keyword contains subschemas in such a way that does not
/// fit with <see cref="ISchemaContainer"/>, <see cref="ISchemaCollector"/>, or
/// <see cref="IKeyedSchemaCollector"/>.
/// </summary>
public interface ICustomSchemaCollector
{
	/// <summary>
	/// The subschemas.
	/// </summary>
	IEnumerable<JsonSchema> Schemas { get; }

	/// <summary>
	/// Gets the indicated subschema.
	/// </summary>
	/// <param name="pointerOld"></param>
	/// <returns>If found, the schema and the number of segments followed to find the subschema; `(null, 0)` otherwise.</returns>
	(JsonSchema? Schema, int SegmentsConsumed) FindSubschema(JsonPointer_Old pointerOld);
}