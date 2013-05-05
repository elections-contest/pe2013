using System;

namespace MandateCalculator
{
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
