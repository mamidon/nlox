namespace nlox
{
	/*
	 * expression → literal
           | unary
           | binary
           | grouping ;
		
		literal    → NUMBER | STRING | "true" | "false" | "nil" ;
		grouping   → "(" expression ")" ;
		unary      → ( "-" | "!" ) expression ;
		binary     → expression operator expression ;
		operator   → "==" | "!=" | "<" | "<=" | ">" | ">="
				   | "+"  | "-"  | "*" | "/" ;
	 */
	public abstract class Expr
	{
		
	}

	public class LiteralExpr : Expr
	{
		public readonly Token Literal;

		public LiteralExpr(Token literal)
		{
			Literal = literal;
		}
	}

	public class GroupingExpr : Expr
	{
		public readonly Expr GroupedExpr;

		public GroupingExpr(Expr groupedExpr)
		{
			GroupedExpr = groupedExpr;
		}
	}

	public class UnaryExpr : Expr
	{
		public readonly Token Operator;
		public readonly Expr Right;

		public UnaryExpr(Token op, Expr right)
		{
			Operator = op;
			Right = right;
		}
	}

	public class BinaryExpr : Expr
	{
		public readonly Expr Left;
		public readonly Token Operator;
		public readonly Expr Right;

		public BinaryExpr(Expr left, Token op, Expr right)
		{
			Left = left;
			Operator = op;
			Right = right;
		}
	}
	
	
	
}