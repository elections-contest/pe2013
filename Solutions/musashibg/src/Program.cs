using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MandateCalculator
{
	class Program
	{
		private const string RegionsFileName = "MIRs.txt";
		private const string PartiesFileName = "Parties.txt";
		private const string CandidatesFileName = "Candidates.txt";
		private const string VoteBatchesFileName = "Votes.txt";
		private const string LotsFileName = "Lots.txt";
		private const string ResultsFileName = "Result.txt";

		private const string RegionPattern = "^(\\d+);\"([^\"]+)\";(\\d+)$";
		private const string PartyPattern = "^(\\d+);\"([^\"]+)\"$";
		private const string CandidatePattern = "^(\\d+);(\\d+);(\\d+);\"([^\"]+)\"$";
		private const string VoteBatchPattern = "^(\\d+);(\\d+);(\\d+)$";
		private const string LotPattern = "^(\\d+)$";

		static void Main(string[] args)
		{
			try
			{
				// Входните данни се зареждат от файловете и се подават на обектът,
				// който ще разпредели мандатите
				var calculator = new MandateCalculator
				{
					Regions = ReadInput(RegionsFileName, RegionPattern, RegionFactory).ToDictionary(m => m.RegionId),
					Parties = ReadInput(PartiesFileName, PartyPattern, PartyFactory).ToDictionary(p => p.PartyId),
					Candidates = ReadInput(CandidatesFileName, CandidatePattern, CandidateFactory),
					VoteBatches = ReadInput(VoteBatchesFileName, VoteBatchPattern, VoteBatchFactory),
					Lots = ReadInput(LotsFileName, LotPattern, LotFactory, true),
				};

				List<Result> results;
				try
				{
					results = calculator.CalculateMandates();
				}
				catch (AmbiguityException ex)
				{
					// При достигане на ситуация, в която разпределението не може да бъде
					// продължено, в Result.txt се записва на първия ред 0, а на следващия -
					// описание на изключителната ситуация
					using (var writer = new StreamWriter(ResultsFileName))
					{
						writer.WriteLine(0);
						writer.WriteLine(ex.Message);
					}
					throw;
				}

				// След като разпределението на мандатите е извършено, резултатите се записват
				// в Result.txt
				ConsoleHelper.WriteSectionCaption("Пълен списък на спечелените мандати");
				using (var writer = new StreamWriter(ResultsFileName))
				{
					foreach (Result result in results)
					{
						writer.WriteLine("{0};{1};{2}", result.RegionId, result.PartyId, result.MandateCount);

						ConsoleHelper.WriteObject(result);
					}
				}
			}
			catch (Exception ex)
			{
				// При възникнало изключение, съобщението му се извежда на стандартния
				// изход и изпълнението се прекратява
				ConsoleHelper.WriteError(ex);
			}
		}

		/// <summary>
		/// Помощен метод за зареждане на входни данни от файл.
		/// </summary>
		/// <typeparam name="T">Тип на входните данни.</typeparam>
		/// <param name="fileName">Име на файла, от който да бъдат заредени
		/// данните.</param>
		/// <param name="linePattern">Регулярен израз, който се използва за
		/// разпознаването на входните данни.</param>
		/// <param name="itemFactory">Функция, която създава обект на базата
		/// на разпознатите входни данни.</param>
		/// <param name="optional">Указва дали изпълнението на приложението да
		/// продължи, ако файлът не съществува. В такъв случай резултатната
		/// колекция е празен.</param>
		/// <returns>Колекция със заредените входни данни.</returns>
		private static List<T> ReadInput<T>(string fileName, string linePattern, Func<Match, T> itemFactory, bool optional = false)
		{
			try
			{
				if (optional && !File.Exists(fileName))
					return new List<T>();

				using (StreamReader reader = File.OpenText(fileName))
				{
					var items = new List<T>();

					int lineIndex = 1;
					string line = reader.ReadLine();
					while (!string.IsNullOrEmpty(line))
					{
						// Текущият ред се разпознава с помощта на подадения регулярен израз
						Match match = Regex.Match(line, linePattern);
						if (!match.Success)
						{
							throw new InputException(
								string.Format("Възникна грешка при опит за прочитане на ред {0} от файл {1}.", lineIndex, fileName));
						}

						try
						{
							// Създава се обект с данните от текущия ред и се добавя в резултатната колекция
							T item = itemFactory(match);
							items.Add(item);
						}
						catch (Exception ex)
						{
							throw new InputException(
								string.Format("Възникна грешка при опит за прочитане на ред {0} от файл {1}.", lineIndex, fileName),
								ex);
						}

						lineIndex++;
						line = reader.ReadLine();
					}

					return items;
				}
			}
			catch (IOException ex)
			{
				throw new InputException(
					string.Format("Възникна грешка при опит за четене на файл {0}.", fileName),
					ex);
			}
		}

		/// <summary>
		/// Помощен метод за създаване на обект с информация за многомандатен
		/// изборен район.
		/// </summary>
		/// <param name="match">Ред от MIRs.txt, разпознат с регулярен
		/// израз.</param>
		/// <returns>Обект със заредената информация за многомандатния изборен
		/// район.</returns>
		private static Region RegionFactory(Match match)
		{
			return new Region
			{
				RegionId = int.Parse(match.Groups[1].Value),
				Name = match.Groups[2].Value,
				MandateCount = int.Parse(match.Groups[3].Value),
			};
		}

		/// <summary>
		/// Помощен метод за създаване на обект с информация за партия или
		/// коалиция.
		/// </summary>
		/// <param name="match">Ред от Parties.txt, разпознат с регулярен
		/// израз.</param>
		/// <returns>Обект със заредената информация за
		/// партията/коалицията.</returns>
		private static Party PartyFactory(Match match)
		{
			return new Party
			{
				PartyId = int.Parse(match.Groups[1].Value),
				Name = match.Groups[2].Value,
			};
		}

		/// <summary>
		/// Помощен метод за създаване на обект с информация за кандидат.
		/// </summary>
		/// <param name="match">Ред от Candidates.txt, разпознат с регулярен
		/// израз.</param>
		/// <returns>Обект със заредената информация за кандидата.</returns>
		private static Candidate CandidateFactory(Match match)
		{
			return new Candidate
			{
				RegionId = int.Parse(match.Groups[1].Value),
				PartyId = int.Parse(match.Groups[2].Value),
				CandidateId = int.Parse(match.Groups[3].Value),
				Name = match.Groups[4].Value,
			};
		}

		/// <summary>
		/// Помощен метод за създаване на обект с информация за група от
		/// действителни гласове.
		/// </summary>
		/// <param name="match">Ред от Votes.txt, разпознат с регулярен
		/// израз.</param>
		/// <returns>Обект със заредената информация за групата от действителни
		/// гласове.</returns>
		private static VoteBatch VoteBatchFactory(Match match)
		{
			return new VoteBatch
			{
				RegionId = int.Parse(match.Groups[1].Value),
				PartyId = int.Parse(match.Groups[2].Value),
				VoteCount = long.Parse(match.Groups[3].Value),
			};
		}

		/// <summary>
		/// Помощен метод за създаване на обект с информация за изтеглен жребий.
		/// </summary>
		/// <param name="match">Ред от Lots.txt, разпознат с регулярен
		/// израз.</param>
		/// <returns>Обект със заредената информация за изтегления
		/// жребий.</returns>
		private static Lot LotFactory(Match match)
		{
			return new Lot
			{
				PartyId = int.Parse(match.Groups[1].Value),
			};
		}
	}
}
