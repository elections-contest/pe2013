using System;

namespace MandateCalculator
{
	public static class ConsoleHelper
	{
		public static void WriteSectionCaption(string caption)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("=== {0}:", caption);
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		public static void WriteObject(object @object, params string[] additionalLines)
		{
			Console.WriteLine(@object);
			foreach (string line in additionalLines)
			{
				Console.WriteLine(line);
			}
			Console.WriteLine();
		}

		public static void WriteError(Exception exception)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(exception.Message);
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}
}
