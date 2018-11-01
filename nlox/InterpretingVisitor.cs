using System;

namespace nlox
{
	public class InterpretingVisitor : Visitor<object>
	{
		public void Interpret(Expr expr)
		{
			try {
				var result = expr.Accept(this);
				Console.Out.WriteLine(stringify(result));
			} catch (LoxRuntimeErrorException loxRuntimeError) {
				Lox.RunTimeError(loxRuntimeError);
			}
		}

		string stringify(object result)
		{
			if (result == null) {
				return "nil";
			}

			var str = result.ToString();

			if (str.EndsWith(".0")) {
				str = str.Substring(0, str.Length - 2);
			}
			
			return str;
		}

		public override object Visit(BinaryExpr expr)
		{
			var left = Evaluate(expr.Left);
			var right = Evaluate(expr.Right);

			switch (expr.Operator.Type) {
				case TokenType.Star:
					VerifyIsDouble(expr.Operator, left, right);
					return (double) left * (double) right;
				case TokenType.Minus:
					VerifyIsDouble(expr.Operator, left, right);
					return (double) left - (double) right;
				case TokenType.Slash:
					VerifyIsDouble(expr.Operator, left, right);
					return (double) left / (double) right;
				case TokenType.Plus:
					if (left is double leftDouble && right is double rightDouble) {
						return leftDouble + rightDouble;
					}

					if (left is string leftStr && right is string rightStr) {
						return leftStr + rightStr;
					}
					
					throw new LoxRuntimeErrorException(expr.Operator, $"Operands must be both numbers or both strings, not '{left}' {expr.Operator.Lexeme} '{right}'");
				case TokenType.Greater:
					VerifyIsDouble(expr.Operator, left, right);
					return (double) left > (double) right;
				case TokenType.GreaterEqual:
					VerifyIsDouble(expr.Operator, left, right);
					return (double) left >= (double) right;
				case TokenType.Less:
					VerifyIsDouble(expr.Operator, left, right);
					return (double) left < (double) right;
				case TokenType.LessEqual:
					VerifyIsDouble(expr.Operator, left, right);
					return (double) left <= (double) right;
				case TokenType.EqualEqual:
					return IsEqual(left, right);
				case TokenType.BangEqual:
					return !IsEqual(left, right);
				default:
					throw new InvalidOperationException($"Unexpected non-binary operator '{expr.Operator.Type}'");
			}
		}

		public override object Visit(UnaryExpr expr)
		{
			var right = Evaluate(expr.Right);

			switch (expr.Operator.Type) {
				case TokenType.Minus:
					VerifyIsDouble(expr.Operator, right);
					return -(double) right;
				case TokenType.Bang:
					return !IsTruthy(right);
				default:
					throw new InvalidOperationException($"Unexpected non-unary operator '{expr.Operator.Type}'");
			}
		}

		public override object Visit(GroupingExpr expr)
		{
			return Evaluate(expr.Expression);
		}

		public override object Visit(LiteralExpr expr)
		{
			return expr.Value;
		}

		object Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}

		bool IsTruthy(object obj)
		{
			if (obj == null) {
				return false;
			}

			if (obj is bool b) {
				return b;
			}

			return true;
		}

		bool IsEqual(object left, object right)
		{
			if (left == null && right == null) {
				return true;
			}

			if (left == null) {
				return false;
			}

			return left.Equals(right);
		}

		void VerifyIsDouble(Token @operator, object obj)
		{
			if (obj is double) {
				return;
			}

			throw new LoxRuntimeErrorException(@operator, $"Operand must be a number, not '{obj}'");
		}
		
		void VerifyIsDouble(Token @operator, object a, object b)
		{
			VerifyIsDouble(@operator, a);
			VerifyIsDouble(@operator, b);
		}
	}
}