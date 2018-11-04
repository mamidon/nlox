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
			return _func(arguments);
		}
	}

	public class LoxFunctionCallable : ILoxCallable
	{
		readonly FunctionStmt _functionStmt;
		readonly LoxEnvironment _closure;

		public LoxFunctionCallable(FunctionStmt stmt, LoxEnvironment closure)
		{
			_functionStmt = stmt;
			_closure = closure;
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

			
			interpreter.Execute(_functionStmt.Body, environment);

			return null;
		}

		public override string ToString()
		{
			return $"[fun {_functionStmt.Name.Lexeme}]";
		}
	}
}