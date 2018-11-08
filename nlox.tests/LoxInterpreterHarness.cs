using System.Collections.Generic;
using Xunit;

namespace nlox.tests
{
	public static class LoxInterpreterHarness
	{
		public static void Run(string source)
		{
			var stmts = new List<Stmt>();
			var scanner = new Scanner(source);
			var tokens = scanner.ScanTokens();
			var parser = new Parser(tokens);
			stmts = parser.Parse();
		
			var visitor = new PrettyPrintingVisitor();
			visitor.Visit(new BlockStmt(stmts));
			var interpreter = new InterpretingVisitor();
			interpreter.Interpret(stmts);

			foreach (var assertion in interpreter.Assertions) {
				Assert.True(assertion.Passed, $"{assertion.Message} -- {assertion.Expected} -- {assertion.Actual}");
			}
		}
	}
}