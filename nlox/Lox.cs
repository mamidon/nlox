using System;
using System.IO;

namespace nlox
{
	public static class Lox
	{
		static bool hadStaticError = false;
		static bool hadRunTimeError = false;
		static InterpretingVisitor Interpreter = new InterpretingVisitor();
		
		static void Main(string[] args)
		{		
			if (args.Length > 1) {
				Console.Out.WriteLine("Usage: nlox [script]");
			}
			else if (args.Length == 1) {
				RunFile(args[0]);
			}
			else {
				RunPrompt();
			}
		}

		static void RunPrompt()
		{
			while (true) {
				hadStaticError = false;
				Console.Out.Write("> ");
				Run(Console.In.ReadLine());
			}
		}

		static void RunFile(string filePath)
		{
			var source = File.ReadAllText(filePath);

			Run(source);

			if (hadStaticError) {
				Environment.Exit(65);
			}
			
			if (hadRunTimeError) {
				Environment.Exit(70);
			}
		}

		static void Run(string source)
		{
			var scanner = new Scanner(source);
			var tokens = scanner.ScanTokens();
			var parser = new Parser(tokens);
			var expr = parser.Parse();

			if (hadStaticError) {
				return;
			}

			Interpreter.Interpret(expr);
		}

		public static void RunTimeError(LoxRuntimeErrorException error)
		{
			Console.Error.WriteLine($"[Line: {error.Operator.Line}] {error.Message}");
		}

		public static void StaticError(int line, string message)
		{
			ReportError(line, string.Empty, message);
		}
		
		static void ReportError(int line, string where, string message)
		{
			hadStaticError = true;
			Console.Out.WriteLine($"[line {line}] Error {where}: {message}");
		}
	}
}
