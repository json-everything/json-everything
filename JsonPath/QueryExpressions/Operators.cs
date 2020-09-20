namespace Json.Path.QueryExpressions
{
	internal static class Operators
	{
		public static readonly IQueryExpressionOperator Exists = new ExistsOperator();
		public static readonly IQueryExpressionOperator EqualTo = new EqualToOperator();
		public static readonly IQueryExpressionOperator NotEqualTo = new NotEqualToOperator();
		public static readonly IQueryExpressionOperator LessThan = new LessThanOperator();
		public static readonly IQueryExpressionOperator LessThanOrEqualTo = new LessThanOrEqualToOperator();
		public static readonly IQueryExpressionOperator GreaterThan = new GreaterThanOperator();
		public static readonly IQueryExpressionOperator GreaterThanOrEqualTo = new GreaterThanOrEqualToOperator();
		public static readonly IQueryExpressionOperator Addition = new AdditionOperator();
		public static readonly IQueryExpressionOperator Subtraction = new SubtractionOperator();
		public static readonly IQueryExpressionOperator Multiplication = new MultiplicationOperator();
		public static readonly IQueryExpressionOperator Division = new DivisionOperator();
		public static readonly IQueryExpressionOperator Modulus = new ModulusOperator();
		public static readonly IQueryExpressionOperator And = new AndOperator();
		public static readonly IQueryExpressionOperator Or = new OrOperator();
	}
}