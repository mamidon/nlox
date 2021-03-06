﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace nlox
{
	public class InterpretingVisitor : IExprVisitor<object>, IStmtVisitor
	{
		LoxEnvironment _currentEnvironment;
		List<LoxAssertionResult> _assertions;
		Dictionary<Expr, int> _scopeMap;
		
		public LoxEnvironment GlobalEnvironment { get; }
		public IReadOnlyList<LoxAssertionResult> Assertions => _assertions;
		
		public InterpretingVisitor()
		{
			GlobalEnvironment = new LoxEnvironment();
			_assertions = new List<LoxAssertionResult>();
			_scopeMap = new Dictionary<Expr, int>();
			_currentEnvironment = GlobalEnvironment;
			
			var start = DateTime.UtcNow.Ticks;
			GlobalEnvironment.Define("clock", new LoxNativeCallable(0, (args) => (double) (DateTime.UtcNow.Ticks - start) / 10000));
			GlobalEnvironment.Define("assert", AssertLoxNativeCallable.CreateAssertLoxNativeCallable(_assertions));
			GlobalEnvironment.Define("stringify", new LoxNativeCallable(1, (arg) => arg[0]?.ToString() ?? "nil"));
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

		public void Execute(IEnumerable<Stmt> statements, LoxEnvironment customEnvironment)
		{
			var holding = _currentEnvironment;

			try {
				_currentEnvironment = customEnvironment;

				foreach (var innerStmt in statements) {
					Execute(innerStmt);
				}
			} finally {
				_currentEnvironment = holding;
			}
		}
		
		public object Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}

		public void Execute(Stmt stmt)
		{
			stmt.Accept(this);
		}
		
		public void Resolve(Expr expr, int distance)
		{
			_scopeMap[expr] = distance;
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

			if (_scopeMap.ContainsKey(expr)) {
				var distance = _scopeMap[expr];
				_currentEnvironment.AssignAt(distance, expr.Name, value);
			} else {
				GlobalEnvironment.Assign(expr.Name, value);
			}
			
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

		public object Visit(LogicalExpr expr)
		{
			var left = Evaluate(expr.Left);

			switch (expr.Operator.Type) {
				case TokenType.And:
					if (!IsTruthy(left)) {
						return left;
					}

					break;
				case TokenType.Or:
					if (IsTruthy(left)) {
						return left;
					}

					break;
			}

			return Evaluate(expr.Right);
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

		public object Visit(ThisExpr expr)
		{
			return LookupVariableByResolution(expr.Keyword, expr);
		}

		public object Visit(CallExpr expr)
		{
			var callee = Evaluate(expr.Callee);

			var arguments = expr.Arguments.Select(Evaluate).ToList();
			
			if(callee is ILoxCallable callable) {
				if (callable.Arity() != arguments.Count) {
					throw new LoxRuntimeErrorException(expr.ClosingParen, $"Callee expects {callable.Arity()} arguments, but was passed {arguments.Count} arguments.");
				}

				try {
					callable.Call(this, arguments);
				} catch (LoxReturnException ex) {
					return ex.Value;
				}
				
			} else {
				throw new LoxRuntimeErrorException(expr.ClosingParen, "Callee is not a function");
			}

			return null;
		}

		public object Visit(GetExpr expr)
		{
			var evaluatedValue = Evaluate(expr.Instance);

			if (evaluatedValue is LoxInstance instance) {
				return instance.Get(expr.Name);
			}

			throw new LoxRuntimeErrorException(expr.Name, "Only Lox class instances can have properties.");
		}

		public object Visit(SetExpr expr)
		{
			var evaluatedValue = Evaluate(expr.Instance);

			if (evaluatedValue is LoxInstance instance) {
				instance.Set(expr.Name, Evaluate(expr.Value));

				return null;
			}

			throw new LoxRuntimeErrorException(expr.Name, "Only Lox class instances can have properties.");
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
			return LookupVariableByResolution(expr.Name, expr);
		}

		public void Visit(BlockStmt stmt)
		{
			Execute(stmt.Statements, _currentEnvironment);
		}

		public void Visit(IfStmt stmt)
		{
			var conditionalResult = Evaluate(stmt.Condition);

			if (IsTruthy(conditionalResult)) {
				Execute(stmt.ThenStatement);
			} else if (stmt.ElseStatement != null) {
				Execute(stmt.ElseStatement);
			}
		}

		public void Visit(FunctionStmt stmt)
		{
			var callable = new LoxFunctionCallable(stmt, _currentEnvironment);

			_currentEnvironment.Define(stmt.Name.Lexeme, callable);
		}

		public void Visit(ClassStmt stmt)
		{
			_currentEnvironment.Define(stmt.Name.Lexeme, null);

			var methods = stmt.Methods.Select(m => new {
				Name = m.Name.Lexeme,
				Method = new LoxFunctionCallable(m, _currentEnvironment, isInitializer: m.Name.Lexeme.Equals("init"))
			}).ToDictionary(tuple => tuple.Name, tuple => tuple.Method);

			var classDefinition = new LoxClass(stmt.Name.Lexeme, methods);
			
			_currentEnvironment.Assign(stmt.Name, classDefinition);
			
		}

		public void Visit(ReturnStmt stmt)
		{
			object value = null;
			if (stmt.Value != null) {
				value = Evaluate(stmt.Value);
			}

			throw new LoxReturnException(value);
		}

		public void Visit(WhileStmt stmt)
		{
			while (IsTruthy(Evaluate(stmt.Condition))) {
				Execute(stmt.BodyStatement);
			}
		}

		public void Visit(VarStmt stmt)
		{
			object value = null;

			if (stmt.Initializer != null) {
				value = Evaluate(stmt.Initializer);
			}
			
			_currentEnvironment.Define(stmt.Name.Lexeme, value);
		}

		public void Visit(PrintStmt stmt)
		{
			Console.Out.WriteLine(stringify(Evaluate(stmt.Expression)));
		}

		public void Visit(ExpressionStmt stmt)
		{
			Evaluate(stmt.Expression);
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

		object LookupVariableByResolution(Token name, Expr expr)
		{
			if (!_scopeMap.ContainsKey(expr)) {
				return GlobalEnvironment.Get(name);
			} else {
				var distance = _scopeMap[expr];
				return _currentEnvironment.GetAt(distance, name);
			}
		}
	}
}