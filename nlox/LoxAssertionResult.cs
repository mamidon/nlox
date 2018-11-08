namespace nlox
{
	public class LoxAssertionResult
	{
		public bool Passed { get; set; }
		public object Expected { get; set; }
		public object Actual { get; set; }
		public string Message { get; set; }
	}
}