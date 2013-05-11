using System;

namespace MandateCalculator
{
	/// <summary>
	/// Изключение, което се хвърля при достигната ситуация, в която
	/// изчислението не може да бъде продължено.
	/// </summary>
	public class AmbiguityException : Exception
	{
		public AmbiguityException()
		{
		}

		public AmbiguityException(string message)
			: base(message)
		{
		}

		public AmbiguityException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
