namespace nlox
{
	public abstract class Visitor<R>
	{
		public abstract R Visit(BinaryExpr expr);
		public abstract R Visit(UnaryExpr expr);
		public abstract R Visit(GroupingExpr expr);
		public abstract R Visit(LiteralExpr expr);
	}
}