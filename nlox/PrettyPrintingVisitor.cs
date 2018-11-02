namespace nlox
{
	public class PrettyPrintingVisitor : IExprVisitor<string>
	{
		public string Visit(AssignExpr expr)
		{
			return $"(= {expr.Name.Lexeme} {expr.Value.Accept(this)}";
		}

		public string Visit(BinaryExpr expr)
		{
			return $"({expr.Operator} {expr.Left.Accept(this)} {expr.Right.Accept(this)})";
		}

		public string Visit(UnaryExpr expr)
		{
			return $"({expr.Operator} {expr.Right.Accept(this)})";
		}

		public string Visit(GroupingExpr expr)
		{
			return $"(group {expr.Expression.Accept(this)})";
		}

		public string Visit(LiteralExpr expr)
		{
			return $"{expr.Value ?? "nil"}";
		}

		public string Visit(VariableExpr expr)
		{
			return $"(identifier '{expr.Name}')";
		}
	}
}