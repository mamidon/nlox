﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace nlox
{	
	/*
program     → declaration* EOF ;

declaration → varDecl
			| funDecl
			| classDecl
            | statement ;
            
varDecl → "var" IDENTIFIER ( "=" expression )? ";" ;
funDecl → "fun" function ;
classDecl → "class" IDENTIFIER "{" function* "}" ;
function → IDENTIFIER "(" parameters? ")" ;
parameters → IDENTIFIER ( "," IDENTIFIER )* ;

statement   → exprStmt
			| ifStmt
			| whileStmt
			| returnStmt
			| forStmt
            | printStmt
            | block ;

exprStmt  → expression ";" ;
printStmt → "print" expression ";" ;
block → "{" declaration* "}" ;
ifStmt    → "if" "(" expression ")" statement ( "else" statement )? ;
whileStmt → "while" "(" expression ")" statement ;
returnStmt → "return" expression? ";" ;
forStmt → "for" "(" 
	( varDecl | exprStmt ";" ) 
	expression? ";" 
	expression? ")" 
	
	statement ;

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
               | call ;
               
call		   → primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
arguments      → "(" expression ( "," expression )* ")" ;
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

				if (MatchNext(TokenType.Fun)) {
					return FunDeclaration("function");
				}

				if (MatchNext(TokenType.Class)) {
					return ClassDeclaration();
				}

				return Statement();
			} catch (LoxStaticErrorException) {
				Synchronize();
				return null;
			}
		}

		Stmt ClassDeclaration()
		{
			var name = Consume(TokenType.Identifier, "Expected a class name.");
			var methods = new List<FunctionStmt>();
			
			Consume(TokenType.LeftBrace, "Expected an opening '{'.");

			while (PeekNext().Type != TokenType.RightBrace && !IsAtEnd()) {
				methods.Add(FunDeclaration("method"));
			}

			Consume(TokenType.RightBrace, "Expected a closing '}'.");

			return new ClassStmt(name, methods);
		}

		FunctionStmt FunDeclaration(string functionType)
		{
			var name = Consume(TokenType.Identifier, $"Expected a {functionType} name.");

			Consume(TokenType.LeftParen, "Expected opening '('.");

			var parameters = new List<Token>();

			if (PeekNext().Type != TokenType.RightParen) {
				do {
					if (parameters.Count > 8) {
						CreateStaticError(parameters[parameters.Count - 1],
							$"Only 8 parameters are supported for {functionType} declarations.");
					}

					parameters.Add(Consume(TokenType.Identifier, "Expected a valid identifier"));
				} while (MatchNext(TokenType.Comma));
			}

			Consume(TokenType.RightParen, "Expected closing ')'.");
			Consume(TokenType.LeftBrace, "Expected opening '}'");
			
			var body = BlockStatement();

			return new FunctionStmt(name, parameters, body.Statements);
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

			if (MatchNext(TokenType.While)) {
				return WhileStatement();
			}

			if (MatchNext(TokenType.Return)) {
				return ReturnStatement();
			}

			if (MatchNext(TokenType.For)) {
				return ForStatement();
			}
			
			return ExpressionStatement();
		}

		Stmt ForStatement()
		{
			Consume(TokenType.LeftParen, "Expected opening '('.");

			Stmt initializer = null;
			if (MatchNext(TokenType.Var)) {
				initializer = VarDeclaration();
			} else if (MatchNext(TokenType.SemiColon)) {
				initializer = null;
			} else {
				initializer = ExpressionStatement();
			}

			Expr conditional = null;
			if (!MatchNext(TokenType.SemiColon)) {
				conditional = Expression();
				Consume(TokenType.SemiColon, "Expecting ';' in for loop");
			}


			Expr incrementer = null;
			if (!MatchNext(TokenType.RightParen)) {
				incrementer = Expression();
				Consume(TokenType.RightParen, "Expected closing ')' in for loop");
			}

			var body = Statement();

			if (incrementer != null) {
				body = new BlockStmt(new List<Stmt> {
					body,
					new ExpressionStmt(incrementer)
				});
			}

			if (conditional == null) {
				conditional = new LiteralExpr(true);
			}
			body = new WhileStmt(conditional, body);

			if (initializer != null) {
				body = new BlockStmt(new List<Stmt> {
					initializer,
					body
				});
			}

			return body;
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

		BlockStmt BlockStatement()
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

		Stmt WhileStatement()
		{
			Consume(TokenType.LeftParen, "Expecting opening '('.");

			var condition = Expression();

			Consume(TokenType.RightParen, "Expecting closing ')'");

			var bodyStatement = Statement();

			return new WhileStmt(condition, bodyStatement);
		}

		Stmt ReturnStatement()
		{
			var token = Previous();
			Expr result = null;
			if (PeekNext().Type != TokenType.SemiColon) {
				result = Expression();
			} 
			
			Consume(TokenType.SemiColon, "Expected ';' after return.");
			return new ReturnStmt(token, result);
		}

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
				} else if (expr is GetExpr getExpr) {
					return new SetExpr(getExpr.Instance, getExpr.Name, value);
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

			return CallExpression();
		}

		Expr CallExpression()
		{
			var expr = Primary();

			while (true) {
				if (MatchNext(TokenType.LeftParen)) {
					expr = FinishCallExpression(expr);
				} else if (MatchNext(TokenType.Dot)) {
					var name = Consume(TokenType.Identifier, "Expected a property identifier after '.'.");
					expr = new GetExpr(expr, name);
				} else {
					break;
				}
			}

			return expr;
		}

		Expr FinishCallExpression(Expr callee)
		{
			var arguments = new List<Expr>();

			if (PeekNext().Type != TokenType.RightParen) {
				do {
					if (arguments.Count > 8) {
						CreateStaticError(PeekNext(), "Cannot have more than 8 arguments for a function.");
					}
					
					arguments.Add(Expression());
				} while (MatchNext(TokenType.Comma));
			}

			var closingToken = Consume(TokenType.RightParen, "Expected closing ')'.");

			return new CallExpr(callee, closingToken, arguments);
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
			
			if (MatchNext(TokenType.This)) {
				return new ThisExpr(Previous());
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