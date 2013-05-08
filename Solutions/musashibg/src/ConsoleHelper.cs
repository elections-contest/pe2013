using System;

namespace MandateCalculator
{
	/// <summary>
	/// Помощен клас за отпечатване на данни на стандартния изход.
	/// </summary>
	public static class ConsoleHelper
	{
		/// <summary>
		/// Отпечатва заглавие на секциа на стандартния изход.
		/// </summary>
		/// <param name="caption">Заглавието, което да бъде отпечатано.</param>
		public static void WriteSectionCaption(string caption)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("=== {0}:", caption);
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		/// <summary>
		/// Отпечатва на стандартния изход представянето на обект в символен
		/// низ, следвано от празен ред. Възможно е да се подадат един или
		/// повече допълнителни низове, които да бъдат отпечатани преди празния
		/// ред.
		/// </summary>
		/// <param name="object">Обекта, чието низово представяне да бъде
		/// отпечатано.</param>
		/// <param name="additionalLines">Нула или повече допълнителни низове,
		/// които да бъдат опечатани преди новия ред.</param>
		public static void WriteObject(object @object, params string[] additionalLines)
		{
			Console.WriteLine(@object);
			foreach (string line in additionalLines)
			{
				Console.WriteLine(line);
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Отпечатва на стандартния изход съобщението на изключение.
		/// </summary>
		/// <param name="exception">Изключението, чието съобщение да бъде
		/// отпечатано.</param>
		public static void WriteError(Exception exception)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(exception.Message);
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}
}
