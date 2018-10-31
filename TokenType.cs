namespace nlox
{
	public enum TokenType
	{
		// whoopsie
		Unknown,
		
		// single characters
		LeftParen, RightParen, LeftBrace, RightBrace, Comma,
		Dot, Minus, Plus, SemiColon, Slash, Star,
		
		// single or double characters
		Bang, BangEqual,
		Equal, EqualEqual,
		Greater, GreaterEqual,
		Less, LessEqual,
		
		// literals
		Identifier, String, Number,
		
		// keywords
		And, Class, Else, False, Fun, For, If, Nil, Or,
		Print, Return, Super, This, True, Var, While,
		
		EndOfFile
	}
}