using System;
using System.IO;

namespace nlox
{
	public static class Lox
	{
		static bool hasError = false;
		
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
				hasError = false;
				Console.Out.Write("> ");
				Run(Console.In.ReadLine());
			}
		}

		static void RunFile(string filePath)
		{
			var source = File.ReadAllText(filePath);

			Run(source);
		}

		static void Run(string source)
		{
			var scanner = new Scanner(source);
			var tokens = scanner.ScanTokens();

			foreach (var token in tokens) {
				Console.Out.WriteLine(token);
			}
		}

		public static void Error(int line, string message)
		{
			ReportError(line, string.Empty, message);
		}
		
		static void ReportError(int line, string where, string message)
		{
			hasError = true;
			Console.Out.WriteLine($"[line {line}] Error {where}: {message}");
		}
	}
}
