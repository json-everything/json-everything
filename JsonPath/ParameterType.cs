using System;

namespace Json.Path;

[Flags]
public enum ParameterType
{
	Unspecified,
	Object,
	Array,
	String,
	Number,
	Boolean,
	Null,
	Nothing
}