using System;
using System.Collections.Generic;

namespace nlox
{
	public interface ILoxCallable
	{
		int Arity();
		object Call(InterpretingVisitor interpreter, List<object> arguments);
	}

	public class LoxNativeCallable : ILoxCallable
	{
		readonly int _arity;
		readonly Func<List<object>, object> _func;
		
		public LoxNativeCallable(int arity, Func<List<object>, object> func)
		{
			_arity = arity;
			_func = func;
		}
		
		public int Arity()
		{
			return _arity;
		}

		public object Call(InterpretingVisitor interpreter, List<object> arguments)
		{
			throw new LoxReturnException(_func(arguments));
		}
	}

	public static class AssertLoxNativeCallable 
	{
		public static LoxNativeCallable CreateAssertLoxNativeCallable(List<LoxAssertionResult> assertions)
		{
			return new LoxNativeCallable(3, args => {
				var result = Assert(args[0] as string, args[1], args[2]);
				assertions.Add(result);
				return null;
			});
		}

		static LoxAssertionResult Assert(string message, object expected, object actual)
		{
			return new LoxAssertionResult {
				Passed = (expected == null && actual == null) || (expected != null && expected.Equals(actual)),
				Actual = actual,
				Expected = expected,
				Message = message
			};
		}
	}

	public class LoxFunctionCallable : ILoxCallable
	{
		readonly FunctionStmt _functionStmt;
		readonly LoxEnvironment _closure;
		readonly bool _isInitializer;

		public LoxFunctionCallable(FunctionStmt stmt, LoxEnvironment closure, bool isInitializer = false)
		{
			_functionStmt = stmt;
			_closure = closure;
			_isInitializer = isInitializer;
		}
		
		public int Arity()
		{
			return _functionStmt.Params.Count;
		}

		public object Call(InterpretingVisitor interpreter, List<object> arguments)
		{
			var environment = new LoxEnvironment(_closure);

			for (var i = 0; i < Arity(); i++) {
				environment.Define(_functionStmt.Params[i].Lexeme, arguments[i]);
			}

			try {
				interpreter.Execute(_functionStmt.Body, environment);
			} catch (LoxReturnException ex) {
				if (_isInitializer) {
					throw new LoxReturnException(_closure.GetAt(0, new Token(TokenType.This, "this", null, 0)));
				}

				throw;
			}

			return null;
		}

		public LoxFunctionCallable Bind(LoxInstance instance)
		{
			var binding = new LoxEnvironment(_closure);
			binding.Bind(instance);
			return new LoxFunctionCallable(_functionStmt, binding, _isInitializer);
		}

		public override string ToString()
		{
			return $"[fun {_functionStmt.Name.Lexeme}]";
		}
	}
}