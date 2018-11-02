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
			var expressionTypes = new[] {
				"Assign : Token Name, Expr Value",
				"Binary   : Expr Left, Token Operator, Expr Right",
				"Grouping : Expr Expression",                      
				"Literal  : object Value",                         
				"Unary    : Token Operator, Expr Right",
				"Variable : Token Name"
			};
			Console.Out.WriteLine(BuildAbstractSyntaxExprTreeCode("Expr", expressionTypes));
			
			var statementTypes = new[] {
				"Block : List<Stmt> Statements",
				"Expression : Expr Expression",
				"Print : Expr Expression",
				"Var : Token Name, Expr Initializer"
			};
			Console.Out.WriteLine(BuildAbstractSyntaxStmtTreeCode("Stmt", statementTypes));
		}

		static string BuildAbstractSyntaxExprTreeCode(string baseClassName, IEnumerable<string> types)
		{
			var builder = new StringBuilder();
			BuildBaseExprClass(builder, baseClassName);

			foreach (var type in types) {
				BuildExprType(builder, type, baseClassName);
			}

			return builder.ToString();
		}

		static void BuildExprType(StringBuilder builder, string type, string baseType)
		{
			var typeName = type.Split(':')[0].Trim();
			var fields = type.Split(':')[1].Trim();
			
			builder.Append($"public class {typeName}{baseType} : {baseType}");
			builder.Append("{");

			var fieldDefinitions = new List<(string FieldType, string FieldName)>();
			foreach (var fieldString in fields.Split(',').Select(f => f.Trim())) {
				var fieldType = fieldString.Split()[0].Trim();
				var fieldName = fieldString.Split()[1].Trim();
				
				fieldDefinitions.Add((fieldType, fieldName));
			}

			foreach (var definition in fieldDefinitions) {
				builder.Append($"public readonly {definition.FieldType} {definition.FieldName};");
			}

			builder.Append($"public {typeName}{baseType}(");

			builder.Append(string.Join(',', fieldDefinitions.Select(d => $"{d.FieldType} {d.FieldName}")));

			builder.Append(")");
			builder.Append("{");

			foreach (var definition in fieldDefinitions) {
				builder.Append($"this.{definition.FieldName} = {definition.FieldName};");
			}
			
			builder.Append("}");

			builder.Append($"public override R Accept<R>(I{baseType}Visitor<R> visitor) {{ return visitor.Visit(this); }}");
			
			builder.Append("}");
		}

		static void BuildBaseExprClass(StringBuilder builder, string baseClassName)
		{
			builder.Append($"public abstract class {baseClassName}");
			builder.Append("{");
			builder.Append($"	public abstract R Accept<R>(I{baseClassName}Visitor<R> visitor);");
			builder.Append("}");
		}
		
		static string BuildAbstractSyntaxStmtTreeCode(string baseClassName, IEnumerable<string> types)
		{
			var builder = new StringBuilder();
			BuildStmtBaseClass(builder, baseClassName);

			foreach (var type in types) {
				BuildStmtType(builder, type, baseClassName);
			}

			return builder.ToString();
		}

		static void BuildStmtType(StringBuilder builder, string type, string baseType)
		{
			var typeName = type.Split(':')[0].Trim();
			var fields = type.Split(':')[1].Trim();
			
			builder.Append($"public class {typeName}{baseType} : {baseType}");
			builder.Append("{");

			var fieldDefinitions = new List<(string FieldType, string FieldName)>();
			foreach (var fieldString in fields.Split(',').Select(f => f.Trim())) {
				var fieldType = fieldString.Split()[0].Trim();
				var fieldName = fieldString.Split()[1].Trim();
				
				fieldDefinitions.Add((fieldType, fieldName));
			}

			foreach (var definition in fieldDefinitions) {
				builder.Append($"public readonly {definition.FieldType} {definition.FieldName};");
			}

			builder.Append($"public {typeName}{baseType}(");

			builder.Append(string.Join(',', fieldDefinitions.Select(d => $"{d.FieldType} {d.FieldName}")));

			builder.Append(")");
			builder.Append("{");

			foreach (var definition in fieldDefinitions) {
				builder.Append($"this.{definition.FieldName} = {definition.FieldName};");
			}
			
			builder.Append("}");

			builder.Append($"public override void Accept(I{baseType}Visitor visitor) {{ visitor.Visit(this); }}");
			
			builder.Append("}");
		}

		static void BuildStmtBaseClass(StringBuilder builder, string baseClassName)
		{
			builder.Append($"public abstract class {baseClassName}");
			builder.Append("{");
			builder.Append($"	public abstract void Accept(I{baseClassName}Visitor visitor);");
			builder.Append("}");
		}
	}
}