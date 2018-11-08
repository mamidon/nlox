using System;
using Xunit;

namespace nlox.tests
{
	public class LoxSemanticTests
	{
		[Fact]
		public void LexicalScope_ClosureEnvironment_Variables_Are_Not_Shadowed_By_Later_Local_Declarations()
		{
			LoxInterpreterHarness.Run(@"
var a = ""global"";
{
  fun showA(expected) {
    assert(""The closure contains the expected value for surrounding data"", expected, a);
  }

  showA(""global"");
  var a = ""block"";
  showA(""global"");
}
");
		}

		[Fact]
		public void Math_Operators_Work()
		{
			LoxInterpreterHarness.Run(@"
    assert(""Basic Addition"", 5, 2 + 3);
    assert(""Basic Subtraction"", 1, 2 - 1);
    assert(""Basic Multiplication"", 9, 3 * 3);
    assert(""Basic Division"", 3, 6 / 2);
    assert(""Basic Negation"", 7, 8 + -1);
");
		}

		[Fact]
		public void Precedence_Math_Operators()
		{
			LoxInterpreterHarness.Run(@"
    assert(""math works"", 12, 3 * (2 + 2));
    assert(""math works"", -2, 3 * (2 / -3));
    assert(""math works"", 2, 3 * (2 / 3));
");
		}
	}
}