using System;
using System.Collections.Generic;
using System.Linq;

namespace nlox
{	
	/*
program     → declaration* EOF ;

declaration → varDecl
            | statement ;
            
varDecl → "var" IDENTIFIER ( "=" expression )? ";" ;

statement   → exprStmt
			| ifStmt
            | printStmt
            | block ;

exprStmt  → expression ";" ;
printStmt → "print" expression ";" ;
block → "{" declaration* "}" ;
ifStmt    → "if" "(" expression ")" statement ( "else" statement )? ;

expression → assignment ;

assignment → identifier "=" assignment
           | logic_or ;
logic_or   → logic_and ( "or" logic_and )* ;
logic_and  → equality ( "and" equality )* ;

equality       → comparison ( ( "!=" | "==" ) comparison )* ;
comparison     → addition ( ( ">" | ">=" | "<" | "<=" ) addition )* ;
addition       → multiplication ( ( "-" | "+" ) multiplication )* ;
multiplication → unary ( ( "/" | "*" ) unary )* ;
unary          → ( "!" | "-" ) unary
               | primary ;
primary        → NUMBER | STRING | "false" | "true" | "nil"
               | "(" expression ")" ;
	 */
	public class Parser
	{
		readonly IReadOnlyList<Token> _tokens;
		int _nextToken;
		
		public Parser(IReadOnlyList<Token> tokens)
		{
			if (!tokens.Any()) {
				throw new ArgumentException("No tokens to parse", nameof(_tokens));
			}
			
			_tokens = tokens;
			_nextToken = 0;
		}

		public List<Stmt> Parse()
		{
			try {
				var statements = new List<Stmt>();

				while (!IsAtEnd()) {
					statements.Add(Declaration());
				}

				return statements;
			} catch (LoxRuntimeErrorException) {
				return null;
			}
		}

		bool IsAtEnd()
		{
			return _tokens[_nextToken].Type == TokenType.EndOfFile;
		}

		Token Previous()
		{
			return _tokens[_nextToken - 1];
		}

		Token PeekNext()
		{
			return _tokens[_nextToken];
		}

		Token ConsumeNext()
		{
			if (IsAtEnd()) {
				return PeekNext();
			}
			
			return _tokens[_nextToken++];
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

		Token Consume(TokenType expected, string message)
		{
			if (!MatchNext(expected)) {
				var badToken = PeekNext();
				throw CreateStaticError(badToken, message);
			}

			return Previous();
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
		
		Stmt Declaration()
		{
			try {
				if (MatchNext(TokenType.Var)) {
					return VarDeclaration();
				}

				return Statement();
			} catch (LoxStaticErrorException) {
				Synchronize();
				return null;
			}
		}

		Stmt VarDeclaration()
		{
			var identifier = Consume(TokenType.Identifier, "Identifier expected.");
			Expr initializer = null;
			
			if (MatchNext(TokenType.Equal)) {
				initializer = Expression();
			}

			Consume(TokenType.SemiColon, "Expected ';' after variable declaration.");
			return new VarStmt(identifier, initializer);
		}

		Stmt Statement()
		{
			if (MatchNext(TokenType.Print)) {
				return PrintStatement();
			}

			if (MatchNext(TokenType.LeftBrace)) {
				return BlockStatement();
			}

			if (MatchNext(TokenType.If)) {
				return IfStatement();
			}
			
			return ExpressionStatement();
		}

		Stmt PrintStatement()
		{
			var expr = Expression();
			Consume(TokenType.SemiColon, "Expected terminating ';'.");
			return new PrintStmt(expr);
		}

		Stmt ExpressionStatement()
		{
			var expr = Expression();
			Consume(TokenType.SemiColon, "Expected terminating ';'.");
			return new ExpressionStmt(expr);
		}

		Stmt BlockStatement()
		{
			var stmts = new List<Stmt>();
			while (PeekNext().Type != TokenType.RightBrace && !IsAtEnd()) {
				stmts.Add(Declaration());
			}

			Consume(TokenType.RightBrace, "Expected enclosing '}'.");

			return new BlockStmt(stmts);
		}

		Stmt IfStatement()
		{
			Consume(TokenType.LeftParen, "Expecting opening '('.");

			var conditionalExpression = Expression();

			Consume(TokenType.RightParen, "Expecting closing ')'.");

			var thenStatement = Statement();
			Stmt elseStatement = null;
			
			if (MatchNext(TokenType.Else)) {
				elseStatement = Statement();
			}

			return new IfStmt(conditionalExpression, thenStatement, elseStatement);
		}
		
		/*
		 * expression → assignment ;

assignment → identifier "=" assignment
           | logic_or ;
logic_or   → logic_and ( "or" logic_and )* ;
logic_and  → equality ( "and" equality )* ;
		 */
		Expr Expression()
		{
			return Assignment();
		}

		Expr Assignment()
		{
			var expr = Or();

			if (MatchNext(TokenType.Equal)) {
				var equals = Previous();
				var value = Assignment();

				if (expr is VariableExpr variableExpr) {
					return new AssignExpr(variableExpr.Name, value);
				}

				CreateStaticError(equals, "Invalid assignment target");
			}

			return expr;
		}

		Expr Or()
		{
			var expr = And();

			while (MatchNext(TokenType.Or)) {
				var @operator = Previous();
				var right = And();
				return new LogicalExpr(expr, @operator, right);
			}

			return expr;
		}

		Expr And()
		{
			var expr = Equality();

			while (MatchNext(TokenType.And)) {
				var @operator = Previous();
				var right = Equality();
				return new LogicalExpr(expr, @operator, right);
			}

			return expr;
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

			if (MatchNext(TokenType.Identifier)) {
				return new VariableExpr(Previous());
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