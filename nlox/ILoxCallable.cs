using System;
using System.Collections.Generic;

namespace nlox
{
	public interface ILoxCallable
	{
		int Arity();
		object Call(InterpretingVisitor interpreter, List<object> arguments);
	}

	public class NativeLoxCallable : ILoxCallable
	{
		readonly int _arity;
		readonly Func<List<object>, object> _func;
		
		public NativeLoxCallable(int arity, Func<List<object>, object> func)
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
}