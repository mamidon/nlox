using System;

namespace nlox
{
	public class LoxReturnException : Exception
	{
		public readonly object Value;

		public LoxReturnException(object value)
		{
			Value = value;
		}
	}
}