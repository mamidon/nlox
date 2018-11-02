﻿using System;
using System.Collections.Generic;

namespace nlox
{
	public class InterpretingVisitor : IExprVisitor<object>, IStmtVisitor
	{
		readonly LoxEnvironment _globalEnvironment;
		LoxEnvironment _currentEnvironment;
		
		public InterpretingVisitor()
		{
			_globalEnvironment = new LoxEnvironment();
			_currentEnvironment = _globalEnvironment;
		}
		
		public void Interpret(IEnumerable<Stmt> statements)
		{
			try {
				foreach (var stmt in statements) {
					Execute(stmt);
				}
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

		public object Visit(AssignExpr expr)
		{
			var value = Evaluate(expr.Value);
			_globalEnvironment.Assign(expr.Name, value);
			return value;
		}

		public object Visit(BinaryExpr expr)
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

		public object Visit(UnaryExpr expr)
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

		public object Visit(GroupingExpr expr)
		{
			return Evaluate(expr.Expression);
		}

		public object Visit(LiteralExpr expr)
		{
			return expr.Value;
		}

		public object Visit(VariableExpr expr)
		{
			return _globalEnvironment.Get(expr.Name);
		}

		public void Visit(BlockStmt stmt)
		{
			var holding = _currentEnvironment;

			try {
				_currentEnvironment = new LoxEnvironment(_currentEnvironment);

				foreach (var innerStmt in stmt.Statements) {
					Execute(innerStmt);
				}
			} finally {
				_currentEnvironment = holding;
			}
		}

		public void Visit(VarStmt stmt)
		{
			object value = null;

			if (stmt.Initializer != null) {
				value = Evaluate(stmt.Initializer);
			}
			
			_globalEnvironment.Define(stmt.Name.Lexeme, value);
		}

		public void Visit(PrintStmt stmt)
		{
			Console.Out.WriteLine(stringify(Evaluate(stmt.Expression)));
		}

		public void Visit(ExpressionStmt stmt)
		{
			Evaluate(stmt.Expression);
		}

		object Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}

		void Execute(Stmt stmt)
		{
			stmt.Accept(this);
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