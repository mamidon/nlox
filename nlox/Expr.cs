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

	public class BinaryExpr : Expr
	{
		public readonly Expr Left;
		public readonly Token Operator;
		public readonly Expr Right;

		public BinaryExpr(Expr Left, Token Operator, Expr Right)
		{
			this.Left = Left;
			this.Operator = Operator;
			this.Right = Right;
		}
	}

	public class GroupingExpr : Expr
	{
		public readonly Expr Expression;

		public GroupingExpr(Expr Expression)
		{
			this.Expression = Expression;
		}
	}

	public class LiteralExpr : Expr
	{
		public readonly object Value;

		public LiteralExpr(object Value)
		{
			this.Value = Value;
		}
	}

	public class UnaryExpr : Expr
	{
		public readonly Token Operator;
		public readonly Expr Right;

		public UnaryExpr(Token Operator, Expr Right)
		{
			this.Operator = Operator;
			this.Right = Right;
		}
	}
}