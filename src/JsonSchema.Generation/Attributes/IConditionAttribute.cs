namespace Json.Schema.Generation;

internal interface IConditionAttribute
{
	string PropertyName { get; set; }
	object? ConditionGroup { get; set; }
}