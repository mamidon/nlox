using System;
using System.Text;

namespace nlox
{
	public class PrettyPrintingVisitor : IExprVisitor<string>, IStmtVisitor
	{
		readonly StringBuilder _builder;
		int _depth;
		
		public PrettyPrintingVisitor()
		{
			_depth = 0;
			_builder = new StringBuilder();
		}
		
		public string Visit(AssignExpr expr)
		{
			return $"(= {expr.Name.Lexeme} {expr.Value.Accept(this)})";
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

		public string Visit(LogicalExpr expr)
		{
			return $"({expr.Operator} {expr.Left.Accept(this)} {expr.Right.Accept(this)})";
		}

		public void Visit(BlockStmt stmt)
		{
			AppendLinePrefixed("(block");
			
			_depth++;
			foreach (var statement in stmt.Statements) {
				statement.Accept(this);
			}
			_depth--;

			AppendLinePrefixed(")");
		}

		public void Visit(IfStmt stmt)
		{
			AppendLinePrefixed($"(if {stmt.Condition.Accept(this)}");
			_depth++;
			stmt.ThenStatement.Accept(this);
			_depth--;
			AppendLinePrefixed(")");
			
			if (stmt.ElseStatement != null) {
				AppendLinePrefixed($"(else");
				_depth++;
				stmt.ElseStatement.Accept(this);
				_depth--;
				AppendLinePrefixed(")");
			}
		}

		public void Visit(VarStmt stmt)
		{
			AppendLinePrefixed($"(define {stmt.Name} {stmt.Initializer.Accept(this)})");
		}

		public void Visit(PrintStmt stmt)
		{
			AppendLinePrefixed($"(print {stmt.Expression.Accept(this)})");
		}

		public void Visit(ExpressionStmt stmt)
		{
			AppendLinePrefixed($"{stmt.Expression.Accept(this)}");
		}

		public override string ToString()
		{
			return _builder.ToString();
		}

		void AppendLinePrefixed(string line)
		{
			_builder.AppendLine(new string('\t', _depth) + line);
		}
	}
}
//{ var foo = nil; { if (foo) { foo = "ran!"; } else { foo = "no"; } } print foo; }