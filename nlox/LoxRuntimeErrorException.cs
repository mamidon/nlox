using System;

namespace nlox
{
	public class LoxRuntimeErrorException : Exception
	{
		public readonly Token Operator;
		public readonly object Operand;
		
		public LoxRuntimeErrorException(Token @operator, object operand)
		{
			Operator = @operator;
			Operand = operand;
		}
	}
}