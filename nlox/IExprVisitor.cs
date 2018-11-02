﻿namespace nlox
{
	public interface IExprVisitor<R>
	{
		R Visit(AssignExpr expr);
		R Visit(BinaryExpr expr);
		R Visit(UnaryExpr expr);
		R Visit(GroupingExpr expr);
		R Visit(LiteralExpr expr);
		R Visit(VariableExpr expr);
	}

	public interface IStmtVisitor
	{
		void Visit(BlockStmt stmt);
		void Visit(VarStmt stmt);
		void Visit(PrintStmt stmt);
		void Visit(ExpressionStmt stmt);
	}
}