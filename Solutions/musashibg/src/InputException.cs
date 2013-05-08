using System;

namespace MandateCalculator
{
	/// <summary>
	/// Изключение, което се хвърля при невалидни входни данни.
	/// </summary>
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
