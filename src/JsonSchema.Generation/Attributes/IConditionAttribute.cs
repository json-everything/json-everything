namespace Json.Schema.Generation;

internal interface IConditionalAttribute
{
	string PropertyName { get; set; }
	object? ConditionGroup { get; set; }
}