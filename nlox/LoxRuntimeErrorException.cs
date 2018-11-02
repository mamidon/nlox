using System;

namespace nlox
{
	public class LoxRuntimeErrorException : Exception
	{
		public readonly Token Operator;
		
		public LoxRuntimeErrorException(Token @operator, string message) : base(message)
		{
			Operator = @operator;
		}
	}
}