using System;

namespace MandateCalculator
{
	public class InputException : Exception
	{
		public InputException()
		{
		}

		public InputException(string message)
			: base(message)
		{
		}

		public InputException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
