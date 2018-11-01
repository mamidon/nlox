using System;
using System.Collections.Generic;
using System.Linq;

namespace nlox
{	
	public class Parser
	{
		readonly IReadOnlyList<Token> Tokens;
		int _nextToken;
		
		public Parser(IReadOnlyList<Token> tokens)
		{
			if (!tokens.Any()) {
				throw new ArgumentException("No tokens to parse", nameof(Tokens));
			}
			
			Tokens = tokens;
			_nextToken = 0;
		}

		public Expr Parse()
		{
			try {
				return Expression();
			} catch (LoxRuntimeErrorException error) {
				return null;
			}
		}

		bool IsAtEnd()
		{
			return Tokens[_nextToken].Type == TokenType.EndOfFile;
		}

		Token Previous()
		{
			return Tokens[_nextToken - 1];
		}

		Token PeekNext()
		{
			return Tokens[_nextToken];
		}

		Token ConsumeNext()
		{
			if (IsAtEnd()) {
				return PeekNext();
			}
			
			return Tokens[_nextToken++];
		}

		bool MatchNext(TokenType expected)
		{
			if (IsAtEnd()) {
				return false;
			}

			if (PeekNext().Type != expected) {
				return false;
			}

			ConsumeNext();
			return true;
		}

		void Consume(TokenType expected, string message)
		{
			if (!MatchNext(expected)) {
				var badToken = PeekNext();
				throw CreateStaticError(badToken, message);
			}
		}

		Exception CreateStaticError(Token badToken, string message)
		{
			Lox.StaticError(badToken.Line, $"At '{badToken.Lexeme}'. {message}");
			return new LoxStaticErrorException(badToken, message);
		}

		void Synchronize()
		{
			ConsumeNext();

			while (!IsAtEnd()) {
				if (Previous().Type == TokenType.SemiColon) {
					return;
				}

				switch (PeekNext().Type) {
					case TokenType.Class:
					case TokenType.Fun:
					case TokenType.Var:
					case TokenType.For:
					case TokenType.If:
					case TokenType.While:
					case TokenType.Print:
					case TokenType.Return:
						return;
				}

				ConsumeNext();
			}
		}
		
		Expr Expression()
		{
			return Equality();
		}

		Expr Equality()
		{
			var expr = Comparison();

			while (MatchNext(TokenType.BangEqual) || MatchNext(TokenType.EqualEqual)) {
				var @operator = Previous();
				var right = Comparison();

				expr = new BinaryExpr(expr, @operator, right);
			}

			return expr;
		}

		Expr Comparison()
		{
			var expr = Addition();

			while (MatchNext(TokenType.Greater) 
			       || MatchNext(TokenType.GreaterEqual) 
			       || MatchNext(TokenType.Less) 
			       || MatchNext(TokenType.LessEqual)) {
				var @operator = Previous();
				var right = Addition();
				expr = new BinaryExpr(expr, @operator, right);
			}

			return expr;
		}

		Expr Addition()
		{
			var expr = Multiplication();

			while (MatchNext(TokenType.Minus) || MatchNext(TokenType.Plus)) {
				var @operator = Previous();
				var right = Multiplication();

				expr = new BinaryExpr(expr, @operator, right);
			}

			return expr;
		}

		Expr Multiplication()
		{
			var expr = Unary();

			while (MatchNext(TokenType.Slash) || MatchNext(TokenType.Star)) {
				var @operator = Previous();
				var right = Unary();

				expr = new BinaryExpr(expr, @operator, right);
			}

			return expr;
		}

		Expr Unary()
		{
			if (MatchNext(TokenType.Bang) || MatchNext(TokenType.Minus)) {
				var @operator = Previous();
				var right = Unary();

				return new UnaryExpr(@operator, right);
			}

			return Primary();
		}

		Expr Primary()
		{
			if (MatchNext(TokenType.False)) {
				return new LiteralExpr(false);
			}

			if (MatchNext(TokenType.True)) {
				return new LiteralExpr(true);
			}

			if (MatchNext(TokenType.Nil)) {
				return new LiteralExpr(null);
			}

			if (MatchNext(TokenType.Number) || MatchNext(TokenType.String)) {
				return new LiteralExpr(Previous().Literal);
			}


			if (MatchNext(TokenType.LeftParen)) {
				var expr = Expression();

				if (!MatchNext(TokenType.RightParen)) {
					Consume(TokenType.LeftParen, "Expecting ')' after opening parenthesis."); // presume paren for now
				}

				return new GroupingExpr(expr);
			}

			throw CreateStaticError(PeekNext(), "Expecting an expression.");
		}
	}
}