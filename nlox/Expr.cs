namespace nlox
{
/*
expression     → equality ;
equality       → comparison ( ( "!=" | "==" ) comparison )* ;
comparison     → addition ( ( ">" | ">=" | "<" | "<=" ) addition )* ;
addition       → multiplication ( ( "-" | "+" ) multiplication )* ;
multiplication → unary ( ( "/" | "*" ) unary )* ;
unary          → ( "!" | "-" ) unary
               | primary ;
primary        → NUMBER | STRING | "false" | "true" | "nil"
               | "(" expression ")" ;
	 */
	public abstract class Expr
	{
		public abstract R Accept<R>(Visitor<R> visitor);
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

		public override R Accept<R>(Visitor<R> visitor)
		{
			return visitor.Visit(this);
		}
	}

	public class GroupingExpr : Expr
	{
		public readonly Expr Expression;

		public GroupingExpr(Expr Expression)
		{
			this.Expression = Expression;
		}

		public override R Accept<R>(Visitor<R> visitor)
		{
			return visitor.Visit(this);
		}
	}

	public class LiteralExpr : Expr
	{
		public readonly object Value;

		public LiteralExpr(object Value)
		{
			this.Value = Value;
		}

		public override R Accept<R>(Visitor<R> visitor)
		{
			return visitor.Visit(this);
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

		public override R Accept<R>(Visitor<R> visitor)
		{
			return visitor.Visit(this);
		}
	}
}