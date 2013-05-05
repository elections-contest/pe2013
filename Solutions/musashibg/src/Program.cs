using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
					using (var writer = new StreamWriter(ResultsFileName))
					{
						writer.WriteLine(0);
						writer.WriteLine(ex.Message);
					}
					throw;
				}

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
				ConsoleHelper.WriteError(ex);
			}
		}

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
						Match match = Regex.Match(line, linePattern);
						if (!match.Success)
						{
							throw new InputException(
								string.Format("Възникна грешка при опит за прочитане на ред {0} от файл {1}.", lineIndex, fileName));
						}

						try
						{
							if (match.Success)
							{
								T item = itemFactory(match);
								items.Add(item);
							}
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

		private static Region RegionFactory(Match match)
		{
			return new Region
			{
				RegionId = int.Parse(match.Groups[1].Value),
				Name = match.Groups[2].Value,
				MandateCount = int.Parse(match.Groups[3].Value),
			};
		}

		private static Party PartyFactory(Match match)
		{
			return new Party
			{
				PartyId = int.Parse(match.Groups[1].Value),
				Name = match.Groups[2].Value,
			};
		}

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

		private static VoteBatch VoteBatchFactory(Match match)
		{
			return new VoteBatch
			{
				RegionId = int.Parse(match.Groups[1].Value),
				PartyId = int.Parse(match.Groups[2].Value),
				VoteCount = int.Parse(match.Groups[3].Value),
			};
		}

		private static Lot LotFactory(Match match)
		{
			return new Lot
			{
				PartyId = int.Parse(match.Groups[1].Value),
			};
		}
	}
}
