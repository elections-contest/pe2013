using System;
using System.Collections.Generic;
using System.Linq;

namespace MandateCalculator
{
	public class MandateCalculator
	{
		private Dictionary<int, Region> _regions;
		private Dictionary<int, Party> _parties;
		private List<Candidate> _candidates;
		private List<VoteBatch> _voteBatches;
		private List<Lot> _lots;
		private List<Result> _results;

		public Dictionary<int, Region> Regions
		{
			get { return _regions; }
			set
			{
				_regions = value;
				if (_regions != null)
				{
					ConsoleHelper.WriteSectionCaption("Заредени многомандатни избирателни райони");
					foreach (Region region in _regions.Values)
					{
						ConsoleHelper.WriteObject(region);
					}
				}
			}
		}

		public Dictionary<int, Party> Parties
		{
			get { return _parties; }
			set
			{
				_parties = value;
				if (_parties != null)
				{
					ConsoleHelper.WriteSectionCaption("Заредени партии и коалиции");
					foreach (Party party in _parties.Values)
					{
						ConsoleHelper.WriteObject(party);
					}
				}
			}
		}

		public List<Candidate> Candidates
		{
			get { return _candidates; }
			set
			{
				_candidates = value;
				if (_candidates != null)
				{
					ConsoleHelper.WriteSectionCaption("Заредени кандидати");
					foreach (Candidate candidate in _candidates)
					{
						ConsoleHelper.WriteObject(candidate);
					}
				}
			}
		}

		public List<VoteBatch> VoteBatches
		{
			get { return _voteBatches; }
			set
			{
				_voteBatches = value;
				if (_voteBatches != null)
				{
					ConsoleHelper.WriteSectionCaption("Заредени групи действителни гласове");
					foreach (VoteBatch voteBatch in _voteBatches)
					{
						ConsoleHelper.WriteObject(voteBatch);
					}
				}
			}
		}

		public List<Lot> Lots
		{
			get { return _lots; }
			set
			{
				_lots = value;
				if (_lots != null)
				{
					ConsoleHelper.WriteSectionCaption("Заредени резултати от изтеглени жребии");
					foreach (Lot lot in _lots)
					{
						ConsoleHelper.WriteObject(lot);
					}
				}
			}
		}

		public List<Result> CalculateMandates()
		{
			PrepareObjects();

			_results = new List<Result>();
			CalculateIndependentCandidateMandates();
			ExcludeLowRankingParties();
			CalculateTotalPartyMandates();
			CalculateInitialRegionalMandates();
			CalculateAdjustedRegionalMandates();
			FillResults();

			return _results;
		}

		private void PrepareObjects()
		{
			foreach (Candidate candidate in Candidates)
			{
				if (Regions.ContainsKey(candidate.RegionId))
					Regions[candidate.RegionId].Candidates.Add(candidate);
				else
					throw new InputException("Съществува кандидат с невалиден номер на МИР.");

				if (Parties.ContainsKey(candidate.PartyId))
					Parties[candidate.PartyId].Candidates.Add(candidate);
				else if (candidate.CandidateId == 1)
					candidate.IsIndependent = true;
				else
					throw new InputException("Съществува независим кандидат с номер в листата, различен от 1.");
			}

			foreach (VoteBatch voteBatch in VoteBatches)
			{
				if (Regions.ContainsKey(voteBatch.RegionId))
					Regions[voteBatch.RegionId].VoteBatches.Add(voteBatch.PartyId, voteBatch);
				else
					throw new InputException("Съществува група действителни гласове с невалиден номер на МИР.");

				if (Parties.ContainsKey(voteBatch.PartyId))
					Parties[voteBatch.PartyId].VoteBatches.Add(voteBatch.RegionId, voteBatch);
			}
		}

		private void CalculateIndependentCandidateMandates()
		{
			ConsoleHelper.WriteSectionCaption("Независими кандидати, които печелят мандати");
			foreach (Region region in Regions.Values)
			{
				Candidate[] independentCandidates = region.Candidates.Where(c => c.IsIndependent).ToArray();
				if (independentCandidates.Length == 0)
					continue;

				decimal regionQuota = region.GetRegionQuota();

				foreach (Candidate candidate in independentCandidates)
				{
					if (region.GetVoteCount(candidate.PartyId) > regionQuota)
					{
						var result = new Result
						{
							RegionId = region.RegionId,
							PartyId = candidate.PartyId,
							MandateCount = 1,
						};
						_results.Add(result);
						region.MandateCount--;

						ConsoleHelper.WriteObject(candidate);
					}
				}
			}

			RemoveCandidates(c => c.IsIndependent);
		}

		private void ExcludeLowRankingParties()
		{
			ConsoleHelper.WriteSectionCaption("Определяне на партии и коалиции, които ще бъдат допуснати до разпределянето на мандати (по чл. 14)");
			int totalVotes = VoteBatches.Sum(vb => vb.VoteCount);
			Console.WriteLine("Общ брой събрани гласове:     {0}", totalVotes);
			decimal votesThreshold = totalVotes * 0.04m;
			Console.WriteLine("Праг от 4% от общите гласове: {0:F}", votesThreshold);
			Console.WriteLine();

			ConsoleHelper.WriteSectionCaption("Партии и коалиции, които са събрали по-малко от 4% от общите гласове");
			foreach (Party party in Parties.Values.ToList())
			{
				int partyVotes = party.GetTotalVotes();
				if (partyVotes < votesThreshold)
				{
					ConsoleHelper.WriteObject(party,
						string.Format("Брой събрани гласове:     {0} ({1:F}%)", partyVotes, (decimal)partyVotes * 100 / totalVotes));

					Parties.Remove(party.PartyId);
				}
			}

			// Премахване на гласовете за отстранените партии/коалиции/независими кандидати за всеки МИР
			foreach (Region region in Regions.Values)
			{
				foreach (int partyId in region.VoteBatches.Keys.ToList())
				{
					if (!Parties.ContainsKey(partyId))
						region.VoteBatches.Remove(partyId);
				}
			}

			// Премахване на кандидатите от листите на отстранените партии/коалиции
			RemoveCandidates(c => !Parties.ContainsKey(c.PartyId));
			// Премахване на гласовете за отстранените партии/коалиции/независими кандидати
			RemoveVoteBatches(vb => !Parties.ContainsKey(vb.PartyId));
		}

		private void CalculateTotalPartyMandates()
		{
			ConsoleHelper.WriteSectionCaption("Разпределение на мандатите на партии и коалиции на национално ниво (по чл. 16)");
			int totalVotes = VoteBatches.Sum(vb => vb.VoteCount);
			Console.WriteLine("Общ брой събрани гласове:         {0}", totalVotes);
			int totalMandates = Regions.Values.Sum(m => m.MandateCount);
			Console.WriteLine("Общ брой мандати за разпределяне: {0}", totalMandates);
			decimal hareQuota = (decimal)totalVotes / totalMandates;
			Console.WriteLine("Квота на Хеър:                    {0:F6}", hareQuota);
			Console.WriteLine();

			#region Разпределяне на мандати по чл. 16, ал. 3

			ConsoleHelper.WriteSectionCaption("Партии и коалиции, които получават мандати по чл. 16, ал. 3");
			int assignedMandates = 0;
			var remainders = new Dictionary<int, decimal>();
			foreach (Party party in Parties.Values)
			{
				int partyVotes = party.GetTotalVotes();
				decimal partyQuotient = partyVotes / hareQuota;
				party.TargetMandateCount = (int)partyQuotient;

				assignedMandates += party.TargetMandateCount;
				remainders.Add(party.PartyId, partyQuotient - party.TargetMandateCount);

				if (party.TargetMandateCount > 0)
				{
					ConsoleHelper.WriteObject(party,
						string.Format("Брой получени мандати:    {0}", party.TargetMandateCount));
				}
			}

			#endregion

			#region Разпределяне на допълнителни мандати по чл. 16, ал. 5 и 6

			ConsoleHelper.WriteSectionCaption("Партии и коалиции, които получават допълнителен мандат по чл. 16, ал. 5 и 6");
			while (assignedMandates < totalMandates)
			{
				decimal maxRemainder = -1.0m;
				var partyIds = new List<int>();
				foreach (KeyValuePair<int, decimal> remainder in remainders)
				{
					if (remainder.Value > maxRemainder)
					{
						partyIds.Clear();
						partyIds.Add(remainder.Key);
						maxRemainder = remainder.Value;
					}
					else if (remainder.Value == maxRemainder)
						partyIds.Add(remainder.Key);
				}

				int assignableMandates = totalMandates - assignedMandates;
				if (partyIds.Count > assignableMandates)
				{
					Console.WriteLine(
						"Достигнат жребий при разпределяне на мандатите на партии и коалиции по чл. 16, ал. 6. Трябва да бъдат изтеглени {0} партии.",
						assignableMandates);
					Console.WriteLine();

					var chosenPartyIds = new List<int>();
					for (int i = 0; i < assignableMandates; i++)
					{
						if (Lots.Count == 0)
							throw new AmbiguityException("Достигнат жребий по чл. 16, ал. 6, но липсва резултат от жребия в Lots.txt.");

						int partyId = Lots[0].PartyId;
						if (chosenPartyIds.Contains(partyId))
							throw new InputException("Резултатът от жребия, указан в Lots.txt, е невалиден (партията вече е била изтеглена).");
						if (!partyIds.Contains(partyId))
							throw new InputException("Резултатът от жребия, указан в Lots.txt, е невалиден (партията не участва в жребия).");

						Lots.RemoveAt(0);
					}
					partyIds = chosenPartyIds;
				}

				foreach (int partyId in partyIds)
				{
					Party party = Parties[partyId];
					party.TargetMandateCount++;
					remainders.Remove(partyId);

					ConsoleHelper.WriteObject(party,
						string.Format("Общ брой получени мандати: {0}", party.TargetMandateCount));
				}

				assignedMandates += partyIds.Count;
			}

			#endregion
		}

		private void CalculateInitialRegionalMandates()
		{
			foreach (Region region in Regions.Values)
			{
				// Пренебрегват се МИР без мандати за разпределяне
				if (region.MandateCount == 0)
					continue;

				ConsoleHelper.WriteSectionCaption(string.Format("Разпределение на мандатите на партии и коалиции в МИР {0} (по чл. 21)", region.RegionId));
				int totalVotes = region.GetTotalVotes();
				Console.WriteLine("Брой събрани гласове в МИР:         {0}", totalVotes);
				Console.WriteLine("Брой мандати за разпределяне в МИР: {0}", region.MandateCount);
				decimal hareQuota = (decimal)totalVotes / region.MandateCount;
				Console.WriteLine("Квота на Хеър:                      {0:F6}", hareQuota);
				Console.WriteLine();

				#region Разпределяне на мандати по чл. 21, ал. 3

				ConsoleHelper.WriteSectionCaption("Партии и коалиции, които получават мандати по чл. 21, ал. 3");
				foreach (Party party in Parties.Values)
				{
					int partyVotes = region.GetVoteCount(party.PartyId);
					decimal partyQuotient = partyVotes / hareQuota;
					int mainMandateCount = (int)partyQuotient;

					var assignment = new MandateAssignment
					{
						RegionId = region.RegionId,
						PartyId = party.PartyId,
						MainMandateCount = mainMandateCount,
						Remainder = partyQuotient - mainMandateCount,
					};

					party.MandateAssignments.Add(assignment);
					region.MandateAssignments.Add(party.PartyId, assignment);

					ConsoleHelper.WriteObject(assignment);
				}

				#endregion

				#region Разпределяне на допълнителни мандати по чл. 21, ал. 5 и 6

				ConsoleHelper.WriteSectionCaption("Партии и коалиции, които получават допълнителен мандат по чл. 21, ал. 5 и 6");
				int assignedMandates = region.GetAssignedMandateCount();
				while (assignedMandates < region.MandateCount)
				{
					decimal maxRemainder = -1.0m;
					var partyIds = new List<int>();
					foreach (MandateAssignment assignment in region.MandateAssignments.Values)
					{
						// Партии, които вече са получили допълнителен мандат на предишна стъпка, не участват в разпределянето
						if (assignment.AdditionalMandate)
							continue;

						if (assignment.Remainder > maxRemainder)
						{
							partyIds.Clear();
							partyIds.Add(assignment.PartyId);
							maxRemainder = assignment.Remainder;
						}
						else if (assignment.Remainder == maxRemainder)
							partyIds.Add(assignment.PartyId);
					}

					int assignableMandates = region.MandateCount - assignedMandates;
					if (partyIds.Count > assignableMandates)
					{
						Console.WriteLine(
							"Достигнат жребий при разпределяне на мандатите на партии и коалиции по чл. 21, ал. 6. Мандатите ще бъдат получени от първите {0} партии с най-малки номера.",
							assignableMandates);
						Console.WriteLine();

						partyIds.Sort();
						partyIds.RemoveRange(assignableMandates, partyIds.Count - assignableMandates);
					}

					foreach (int partyId in partyIds)
					{
						MandateAssignment assignment = region.MandateAssignments[partyId];
						assignment.AdditionalMandate = true;

						ConsoleHelper.WriteObject(assignment);
					}

					assignedMandates = region.GetAssignedMandateCount();
				}

				#endregion
			}
		}

		private void CalculateAdjustedRegionalMandates()
		{
			ConsoleHelper.WriteSectionCaption("Преразпределяне на допълнителните мандати на партии и коалиции в многомандатните избирателни райони (по чл. 22 - 27)");

			HashSet<int> underassignedPartyIds;
			HashSet<int> overassignedPartyIds;
			var ignoredRegionIds = new HashSet<int>();

			while (FindAssignmentsToAdjust(out underassignedPartyIds, out overassignedPartyIds))
			{
				MandateAssignment minAssignment;
				Region region;

				#region Намиране на най-малкия остатък, за който е разпределен допълнителен мандат (чл. 24 - 26)

				do
				{
					decimal minRemainder = 1.0m;
					minAssignment = null;
					foreach (int partyId in overassignedPartyIds)
					{
						Party party = Parties[partyId];
						foreach (MandateAssignment assignment in party.MandateAssignments)
						{
							// Разглеждат се само разпределения на допълнителни мандати
							if (!assignment.AdditionalMandate)
								continue;

							// Пренебрегват се партии, чиито разпределени допълнителни мандати са били вече отнети
							if (assignment.AlreadyAdjusted)
								continue;

							// Пренебрегват се МИР, които са изключени по чл. 26
							if (ignoredRegionIds.Contains(assignment.RegionId))
								continue;

							if (assignment.Remainder < minRemainder)
							{
								minRemainder = assignment.Remainder;
								minAssignment = assignment;
							}
							else if (assignment.Remainder == minRemainder
										&& (minAssignment == null || assignment.PartyId < minAssignment.PartyId))
							{
								minAssignment = assignment;
							}
						}
					}

					Console.WriteLine("Намерен разпределен допълнителен мандат за най-малък остатък:");
					Console.WriteLine("Номер на МИР:         {0}", minAssignment.RegionId);
					ConsoleHelper.WriteObject(minAssignment);

					region = Regions[minAssignment.RegionId];

					// Ако не е намерена партия, на която може да бъде преразпределен допълнителен мандат
					// в този МИР, районът се изключва от преразпределянето (по чл. 26)
					if (!region.CheckCanAssignAdditionalMandate(underassignedPartyIds))
					{
						ignoredRegionIds.Add(region.RegionId);
						region = null;

						Console.WriteLine("Няма възможност за преразпределяне на мандатите в този МИР. Изключва се районът от по-нататъчното преразпределяне (по чл. 26).");
						Console.WriteLine();
					}
				}
				while (region == null);

				#endregion

				#region Намиране на следващия по големина остатък, за който може да бъде разпределен допълнителен мандат (чл. 27)

				decimal maxRemainder = -1.0m;
				var eligiblePartyIds = new List<int>();
				foreach (MandateAssignment assignment in region.MandateAssignments.Values)
				{
					// Разглеждат се само партии, на които не е разпределен допълнителен мандат в този МИР
					if (assignment.AdditionalMandate)
						continue;

					// Пренебрегват се партии, чиито разпределени допълнителни мандати са били вече отнети
					if (assignment.AlreadyAdjusted)
						continue;

					if (assignment.Remainder > maxRemainder)
					{
						maxRemainder = assignment.Remainder;
						eligiblePartyIds.Clear();
						eligiblePartyIds.Add(assignment.PartyId);
					}
					else if (assignment.Remainder == maxRemainder)
						eligiblePartyIds.Add(assignment.PartyId);
				}

				if (eligiblePartyIds.Count > 1)
					throw new AmbiguityException("При преразпределяне на допълнителен мандат по чл. 27 са достигнати повече от един равни максимални неудовлетворени с допълнителен мандат остатъци.");


				#endregion

				minAssignment.AdditionalMandate = false;
				minAssignment.AlreadyAdjusted = true;
				MandateAssignment eligibleAssignment = region.MandateAssignments[eligiblePartyIds[0]];
				eligibleAssignment.AdditionalMandate = true;
				Console.WriteLine("Намерен следващ най-голям остатък, на който бива разпределен допълнителен мандат в МИР:");
				ConsoleHelper.WriteObject(eligibleAssignment);
			}
		}

		private void FillResults()
		{
			foreach (Region region in Regions.Values)
			{
				foreach (MandateAssignment assignment in region.MandateAssignments.Values)
				{
					// Пренебрегват се партиите, на които не са разпределени мандати в този МИР
					if (assignment.TotalMandateCount == 0)
						continue;

					var result = new Result
					{
						RegionId = region.RegionId,
						PartyId = assignment.PartyId,
						MandateCount = assignment.TotalMandateCount,
					};
					_results.Add(result);
				}
			}
		}

		private void RemoveCandidates(Func<Candidate, bool> predicate)
		{
			_candidates = Candidates.Where(c => !predicate(c)).ToList();
		}

		private void RemoveVoteBatches(Func<VoteBatch, bool> predicate)
		{
			_voteBatches = VoteBatches.Where(vb => !predicate(vb)).ToList();
		}

		private bool FindAssignmentsToAdjust(out HashSet<int> underassignedPartyIds, out HashSet<int> overassignedPartyIds)
		{
			underassignedPartyIds = new HashSet<int>();
			overassignedPartyIds = new HashSet<int>();

			foreach (Party party in Parties.Values)
			{
				int assignedMandateCount = party.GetAssignedMandateCount();
				if (assignedMandateCount < party.TargetMandateCount)
					underassignedPartyIds.Add(party.PartyId);
				else if (assignedMandateCount > party.TargetMandateCount)
					overassignedPartyIds.Add(party.PartyId);
			}

			// Ако няма партии, получили по-малко от очаквания брой мандати,
			// то няма да има и такива, получили повече от очаквания брой
			return underassignedPartyIds.Count > 0;
		}
	}
}
