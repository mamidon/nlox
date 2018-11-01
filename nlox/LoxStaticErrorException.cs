using System;

namespace nlox
{
	public class LoxStaticErrorException : Exception
	{
		public readonly Token Token;

		public LoxStaticErrorException(Token token, string message) : base(message)
		{
			Token = token;
		}
	}
}