using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateAst
{
	class Program
	{
		static void Main(string[] args)
		{
			var types = new[] {
				"Binary   : Expr Left, Token Operator, Expr Right",
				"Grouping : Expr Expression",                      
				"Literal  : object Value",                         
				"Unary    : Token Operator, Expr Right"  
			};
			Console.Out.Write(BuildAbstractSyntaxTreeCode("Expr", types));
		}

		static string BuildAbstractSyntaxTreeCode(string baseClassName, IEnumerable<string> types)
		{
			var builder = new StringBuilder();
			BuildBaseClass(builder, baseClassName);

			foreach (var type in types) {
				BuildType(builder, type, baseClassName);
			}

			return builder.ToString();
		}

		static void BuildType(StringBuilder builder, string type, string baseType)
		{
			var typeName = type.Split(':')[0].Trim();
			var fields = type.Split(':')[1].Trim();
			
			builder.AppendLine($"public class {typeName}{baseType} : {baseType}");
			builder.AppendLine("{");

			var fieldDefinitions = new List<(string FieldType, string FieldName)>();
			foreach (var fieldString in fields.Split(',').Select(f => f.Trim())) {
				var fieldType = fieldString.Split()[0].Trim();
				var fieldName = fieldString.Split()[1].Trim();
				
				fieldDefinitions.Add((fieldType, fieldName));
			}

			foreach (var definition in fieldDefinitions) {
				builder.AppendLine($"public readonly {definition.FieldType} {definition.FieldName};");
			}

			builder.Append($"public {typeName}{baseType}(");

			builder.Append(string.Join(',', fieldDefinitions.Select(d => $"{d.FieldType} {d.FieldName}")));

			builder.AppendLine(")");
			builder.AppendLine("{");

			foreach (var definition in fieldDefinitions) {
				builder.AppendLine($"this.{definition.FieldName} = {definition.FieldName};");
			}
			
			builder.AppendLine("}");
			builder.AppendLine("}");
		}

		static void BuildBaseClass(StringBuilder builder, string baseClassName)
		{
			builder.AppendLine($"public abstract class {baseClassName}");
			builder.AppendLine("{");
			builder.AppendLine("}");
		}
	}
}