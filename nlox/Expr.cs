using System.Collections.Generic;

namespace nlox
{
	public abstract class Expr
	{
		public abstract R Accept<R>(IExprVisitor<R> visitor);
	}

	public class AssignExpr : Expr
	{
		public readonly Token Name;
		public readonly Expr Value;

		public AssignExpr(Token Name, Expr Value)
		{
			this.Name = Name;
			this.Value = Value;
		}

		public override R Accept<R>(IExprVisitor<R> visitor)
		{
			return visitor.Visit(this);
		}
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

		public override R Accept<R>(IExprVisitor<R> visitor)
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

		public override R Accept<R>(IExprVisitor<R> visitor)
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

		public override R Accept<R>(IExprVisitor<R> visitor)
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

		public override R Accept<R>(IExprVisitor<R> visitor)
		{
			return visitor.Visit(this);
		}
	}

	public class VariableExpr : Expr
	{
		public readonly Token Name;

		public VariableExpr(Token Name)
		{
			this.Name = Name;
		}

		public override R Accept<R>(IExprVisitor<R> visitor)
		{
			return visitor.Visit(this);
		}
	}

	public class LogicalExpr : Expr
	{
		public readonly Expr Left;
		public readonly Token Operator;
		public readonly Expr Right;

		public LogicalExpr(Expr Left, Token Operator, Expr Right)
		{
			this.Left = Left;
			this.Operator = Operator;
			this.Right = Right;
		}

		public override R Accept<R>(IExprVisitor<R> visitor)
		{
			return visitor.Visit(this);
		}
	}

	public abstract class Stmt
	{
		public abstract void Accept(IStmtVisitor visitor);
	}

	public class BlockStmt : Stmt
	{
		public readonly List<Stmt> Statements;

		public BlockStmt(List<Stmt> Statements)
		{
			this.Statements = Statements;
		}

		public override void Accept(IStmtVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class ExpressionStmt : Stmt
	{
		public readonly Expr Expression;

		public ExpressionStmt(Expr Expression)
		{
			this.Expression = Expression;
		}

		public override void Accept(IStmtVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class PrintStmt : Stmt
	{
		public readonly Expr Expression;

		public PrintStmt(Expr Expression)
		{
			this.Expression = Expression;
		}

		public override void Accept(IStmtVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class VarStmt : Stmt
	{
		public readonly Token Name;
		public readonly Expr Initializer;

		public VarStmt(Token Name, Expr Initializer)
		{
			this.Name = Name;
			this.Initializer = Initializer;
		}

		public override void Accept(IStmtVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class IfStmt : Stmt
	{
		public readonly Expr Condition;
		public readonly Stmt ThenStatement;
		public readonly Stmt ElseStatement;

		public IfStmt(Expr Condition, Stmt ThenStatement, Stmt ElseStatement)
		{
			this.Condition = Condition;
			this.ThenStatement = ThenStatement;
			this.ElseStatement = ElseStatement;
		}

		public override void Accept(IStmtVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class WhileStmt : Stmt
	{
		public readonly Expr Condition;
		public readonly Stmt BodyStatement;

		public WhileStmt(Expr Condition, Stmt BodyStatement)
		{
			this.Condition = Condition;
			this.BodyStatement = BodyStatement;
		}

		public override void Accept(IStmtVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}