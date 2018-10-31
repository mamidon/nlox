using System;
using System.Text;

namespace GenerateAst
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 2) {
				Console.Error.WriteLine("Usage: GenerateAst <output directory>");
				return;
			}

			var outputDirectory = args[1];
			
		}

		static string BuildAbstractSyntaxTreeCode()
		{
			var builder = new StringBuilder();
			return null;
		}
	}
}