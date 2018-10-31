using System;
using System.Collections.Generic;
using System.Linq;

namespace nlox
{
	public class Scanner
	{
		static readonly Dictionary<string, TokenType> ReservedWords = new Dictionary<string, TokenType> {
			{"", TokenType.And},
			{"class", TokenType.Class},
			{"else", TokenType.Else},
			{"false", TokenType.False},
			{"for", TokenType.For},
			{"fun", TokenType.Fun},
			{"if", TokenType.If},
			{"nil", TokenType.Nil},
			{"or", TokenType.Or},
			{"print", TokenType.Print},
			{"return", TokenType.Return},
			{"super", TokenType.Super},
			{"this", TokenType.This},
			{"true", TokenType.True},
			{"var", TokenType.Var},
			{"while", TokenType.While},
		};
		
		readonly string Source;
		readonly List<Token> Tokens;
		int _line;
		int _startOfNextLexeme;
		int _currentCharacter;

		public Scanner(string source)
		{
			Tokens = new List<Token>();
			Source = source;
			_line = _startOfNextLexeme = _currentCharacter = 0;
		}
		
		public IReadOnlyList<Token> ScanTokens()
		{
			if (Tokens.Any()) {
				return Tokens;
			}
			
			while (!IsAtEnd()) {
				_startOfNextLexeme = _currentCharacter;
				ScanToken();
			}

			Tokens.Add(new Token(TokenType.EndOfFile, string.Empty, null, _line));
			
			return new Token[0];
		}

		void ScanToken()
		{
			var character = Consume();

			switch (character) {
				case '(':
					ProduceToken(TokenType.LeftParen);
					break;
				case ')':
					ProduceToken(TokenType.RightParen);
					break;
				case '{':
					ProduceToken(TokenType.LeftBrace);
					break;
				case '}':
					ProduceToken(TokenType.RightBrace);
					break;
				case ',':
					ProduceToken(TokenType.Comma);
					break;
				case '.':
					ProduceToken(TokenType.Dot);
					break;
				case '-':
					ProduceToken(TokenType.Minus);
					break;
				case '+':
					ProduceToken(TokenType.Plus);
					break;
				case ';':
					ProduceToken(TokenType.SemiColon);
					break;
				case '*':
					ProduceToken(TokenType.Star);
					break;
				case '!':
					ProduceToken(MaybeConsume('=') ? TokenType.BangEqual : TokenType.Bang);
					break;
				case '=':
					ProduceToken(MaybeConsume('=') ? TokenType.EqualEqual : TokenType.Equal);
					break;
				case '<':
					ProduceToken(MaybeConsume('=') ? TokenType.LessEqual : TokenType.Less);
					break;
				case '>':
					ProduceToken(MaybeConsume('=') ? TokenType.GreaterEqual : TokenType.Greater);
					break;
				case '/':
					if (MaybeConsume('/')) {
						while (Peek() != '\n' && !IsAtEnd()) {
							Consume();
						}
					} else {
						ProduceToken(TokenType.Slash);
					}

					break;
				
				case '"':
					ProduceStringLiteral();
					break;
				case char digit when char.IsDigit(digit):
					ProduceNumericLiteral();
					break;
				case char alpha when char.IsLetter(alpha):
					ProduceIdentifier();
					break;
				case ' ':
				case '\t':
				case '\r':
					break;
				
				case '\n':
					_line++;
					break;
					
				default:
					Lox.Error(_line, $"Unexpected character '{Source[_currentCharacter]}'.");
					break;
			}
		}

		void ProduceIdentifier()
		{
			while (char.IsLetterOrDigit(Peek())) {
				Consume();
			}

			var length = _currentCharacter - _startOfNextLexeme;
			var identifier = Source.Substring(_startOfNextLexeme, length);

			if (ReservedWords.ContainsKey(identifier.ToLower())) {
				ProduceToken(ReservedWords[identifier.ToLower()]);
			} else {
				ProduceToken(TokenType.Identifier);	
			}
		}

		void ProduceNumericLiteral()
		{
			while (char.IsDigit(Peek())) {
				Consume();
			}

			if (Peek() == '.' && char.IsDigit(PeekNext())) {
				Consume();

				while (char.IsDigit(Peek())) {
					Consume();
				}
			}

			var length = _currentCharacter - _startOfNextLexeme;
			var literal = double.Parse(Source.Substring(_startOfNextLexeme, length));
			
			ProduceToken(TokenType.Number, literal);
		}

		void ProduceStringLiteral()
		{
			while (Peek() != '"' && !IsAtEnd()) {
				if (Peek() == '\n') {
					_line++;
				}
				
				Consume();
			}

			if (IsAtEnd()) {
				Lox.Error(_line, "Unterminated string.");
			}

			Consume();
			var length = _currentCharacter - _startOfNextLexeme;
			var literal = Source.Substring(_startOfNextLexeme + 1, length - 2);
			ProduceToken(TokenType.String, literal);
		}

		char Peek()
		{
			if (IsAtEnd()) {
				return '\0';
			}
			
			return Source[_currentCharacter];
		}

		char PeekNext()
		{
			if (_currentCharacter + 1 >= Source.Length) {
				return '\0';
			}

			return Source[_currentCharacter + 1];
		}

		void ProduceToken(TokenType tokenType)
		{
			ProduceToken(tokenType, null);
		}

		void ProduceToken(TokenType tokenType, object literal)
		{
			var length = _currentCharacter - _startOfNextLexeme;
			var token = new Token(tokenType, Source.Substring(_startOfNextLexeme, length), literal, _line);
			
			Console.Out.WriteLine(token.ToString());
			
			Tokens.Add(token);
		}

		bool IsAtEnd()
		{
			return _currentCharacter >= Source.Length;
		}

		char Consume()
		{
			_currentCharacter++;
			return Source[_currentCharacter-1];
		}

		bool MaybeConsume(char expected)
		{
			if (IsAtEnd()) {
				return false;
			}

			if (Source[_currentCharacter] != expected) {
				return false;
			}

			Consume();

			return true;
		}
	}
}