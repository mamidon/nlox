namespace nlox
{
	public class PrettyPrintingVisitor : Visitor<string>
	{
		public override string Visit(BinaryExpr expr)
		{
			return $"({expr.Operator} {expr.Left.Accept(this)} {expr.Right.Accept(this)})";
		}

		public override string Visit(UnaryExpr expr)
		{
			return $"({expr.Operator} {expr.Right.Accept(this)})";
		}

		public override string Visit(GroupingExpr expr)
		{
			return $"(group {expr.Expression.Accept(this)})";
		}

		public override string Visit(LiteralExpr expr)
		{
			return $"{expr.Value ?? "nil"}";
		}
	}
}