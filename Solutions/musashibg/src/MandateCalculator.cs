using System;
using System.Collections.Generic;
using System.Linq;

namespace MandateCalculator
{
	/// <summary>
	/// Реализира програмно Методиката за определяне резултатите от гласуването
	/// за народни представители.
	/// </summary>
	public class MandateCalculator
	{
		private Dictionary<int, Region> _regions;
		private Dictionary<int, Party> _parties;
		private List<Candidate> _candidates;
		private List<VoteBatch> _voteBatches;
		private List<Lot> _lots;
		private List<Result> _results;
		private Dictionary<int, decimal> _nationalRemainders;

		/// <summary>
		/// Колекция от многомандатни изборни райони, които ще участват в
		/// разпределянето. Ключът на колекцията е номерът на многомандатния
		/// изборен район.
		/// </summary>
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

		/// <summary>
		/// Колекция от партии и коалиции, които са участвали в гласуването.
		/// Ключът на колекцията е номерът на партията/коалицията.
		/// </summary>
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

		/// <summary>
		/// Колекция от кандидати за народни представители.
		/// </summary>
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

		/// <summary>
		/// Колекция от групи действителни гласове.
		/// </summary>
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

		/// <summary>
		/// Колекция от изтеглени жребии.
		/// </summary>
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

		/// <summary>
		/// Изчислява разпределението на мандатите според Методиката и връща
		/// резултатите.
		/// </summary>
		/// <returns>Колекция от резултатите от разпределението.</returns>
		public List<Result> CalculateMandates()
		{
			PrepareObjects();

			_results = new List<Result>();
			CalculateIndependentCandidateMandates();
			ExcludeLowRankingParties();
			CalculateNationalMandates();
			CalculateInitialRegionalMandates();
			CalculateAdjustedRegionalMandates();
			FillResults();

			return _results;
		}

		/// <summary>
		/// Подготвя заредените входни данни за употреба в следващите стъпки
		/// от разпределянето.
		/// </summary>
		private void PrepareObjects()
		{
			// Всеки кандидатите се добавя в колекциите на съответните многомандатен изборен
			// район и партия/коалиция, от която е бил издигнат. Маркират се независимите кандидати.
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

			// Всяка група от действителни гласове се  добавя в колекциите на съответните
			// многомандатен изборен район и партия/коалиция
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

		/// <summary>
		/// Изчислява разпределенитето на мандати на независимите кандидати в
		/// многомандатните изборни райони (по чл. 9).
		/// </summary>
		private void CalculateIndependentCandidateMandates()
		{
			ConsoleHelper.WriteSectionCaption("Независими кандидати, които печелят мандати");
			foreach (Region region in Regions.Values)
			{
				// Отсяват се независимите кандидати в района
				Candidate[] independentCandidates = region.Candidates.Where(c => c.IsIndependent).ToArray();
				if (independentCandidates.Length == 0)
					continue;

				// Изчислява се районната квота
				decimal regionQuota = region.GetRegionQuota();

				foreach (Candidate candidate in independentCandidates)
				{
					if (region.GetVoteCount(candidate.PartyId) >= regionQuota)
					{
						// В случай че независимият кандидат е събрал необходимия брой гласове,
						// за него се разпределя мандат в съответния многомандатен изборен район
						var result = new Result
						{
							RegionId = region.RegionId,
							PartyId = candidate.PartyId,
							MandateCount = 1,
						};
						_results.Add(result);

						// Разпределеният мандат не участва в пропорционалното разпределение на мандати
						region.MandateCount--;

						ConsoleHelper.WriteObject(candidate);
					}
				}
			}

			// Всички независими кандидати се премахват от колекцията, тъй като те няма
			// да участват в по-нататъчното разпределение на мандати
			RemoveCandidates(c => c.IsIndependent);
		}

		/// <summary>
		/// Определя и изключва партиите и коалициите, които са получили
		/// по-малко от 4% от общия брой действителни гласове в гласуването
		/// (по чл. 10).
		/// </summary>
		private void ExcludeLowRankingParties()
		{
			// Определя се общият брой действителни гласове в гласуването,
			// както и 4% от тях
			ConsoleHelper.WriteSectionCaption("Определяне на партии и коалиции, които ще бъдат допуснати до разпределянето на мандати (по чл. 14)");
			long totalVoteCount = VoteBatches.Sum(vb => vb.VoteCount);
			Console.WriteLine("Общ брой събрани гласове:     {0}", totalVoteCount);
			decimal voteThreshold = totalVoteCount * 0.04m;
			Console.WriteLine("Праг от 4% от общите гласове: {0:F}", voteThreshold);
			Console.WriteLine();

			// Изключват се всички партии и коалиции, сбрали по-малко от нужния брой гласове
			ConsoleHelper.WriteSectionCaption("Партии и коалиции, които са събрали по-малко от 4% от общите гласове");
			foreach (Party party in Parties.Values.ToList())
			{
				long partyVoteCount = party.GetTotalVoteCount();
				if (partyVoteCount < voteThreshold)
				{
					ConsoleHelper.WriteObject(party,
						string.Format("Брой събрани гласове:     {0} ({1:F}%)", partyVoteCount, (decimal)partyVoteCount * 100 / totalVoteCount));

					Parties.Remove(party.PartyId);
				}
			}

			// Премахват се гласовете за отстранените партии/коалиции/независими кандидати за
			// всеки многомандатен изборен район
			foreach (Region region in Regions.Values)
			{
				foreach (int partyId in region.VoteBatches.Keys.ToList())
				{
					if (!Parties.ContainsKey(partyId))
						region.VoteBatches.Remove(partyId);
				}
			}

			// Премахват се кандидатите от листите на отстранените партии/коалиции
			RemoveCandidates(c => !Parties.ContainsKey(c.PartyId));
			// Премахват се гласовете за отстранените партии/коалиции/независими кандидати
			RemoveVoteBatches(vb => !Parties.ContainsKey(vb.PartyId));
		}

		/// <summary>
		/// Изичслява разпределението на мандати на партии и коалиции на
		/// национално ниво (по чл. 16).
		/// </summary>
		private void CalculateNationalMandates()
		{
			// Изчислява се общият брой действителни гласове, събрани от партиите,
			// определени в чл. 14, общият брой мандати за разпределяне във всички
			// многомандатни изборни райони, както и квотата на Хеър
			ConsoleHelper.WriteSectionCaption("Разпределение на мандатите на партии и коалиции на национално ниво (по чл. 16)");
			long totalVoteCount = VoteBatches.Sum(vb => vb.VoteCount);
			Console.WriteLine("Общ брой събрани гласове:         {0}", totalVoteCount);
			int totalMandateCount = Regions.Values.Sum(m => m.MandateCount);
			Console.WriteLine("Общ брой мандати за разпределяне: {0}", totalMandateCount);
			decimal hareQuota = (decimal)totalVoteCount / totalMandateCount;
			Console.WriteLine("Квота на Хеър:                    {0:F6}", hareQuota);
			Console.WriteLine();

			_nationalRemainders = new Dictionary<int, decimal>();

			// Разпределят се мандатите по чл. 16, ал. 3
			AssignBaseNationalMandates(hareQuota);

			// Разпределят се допълнителните мандати по чл. 16, ал. 5 и 6
			AssignAdditionalNationalMandates(totalMandateCount);
		}

		/// <summary>
		/// Извършва разпределение на основни мандати на партии и коалиции на
		/// национално ниво (по чл. 16, ал. 3).
		/// </summary>
		/// <param name="hareQuota">Предварително изчислената квота на
		/// Хеър.</param>
		private void AssignBaseNationalMandates(decimal hareQuota)
		{
			ConsoleHelper.WriteSectionCaption("Партии и коалиции, които получават мандати по чл. 16, ал. 3");
			foreach (Party party in Parties.Values)
			{
				long partyVoteCount = party.GetTotalVoteCount();
				decimal partyQuotient = partyVoteCount / hareQuota;
				party.NationalMandateCount = (int)partyQuotient;

				_nationalRemainders.Add(party.PartyId, partyQuotient - party.NationalMandateCount);

				if (party.NationalMandateCount > 0)
				{
					ConsoleHelper.WriteObject(party,
						string.Format("Брой получени мандати:    {0}", party.NationalMandateCount));
				}
			}
		}

		/// <summary>
		/// Извършва разпределение на допълнителни мандати на партии и коалиции
		/// на национално ниво (по чл. 16, ал. 5 и 6).
		/// </summary>
		/// <param name="totalMandateCount">Общ брой мандати, които трябва да
		/// бъдат разпределени във всички многомандатни изборни райони.</param>
		private void AssignAdditionalNationalMandates(int totalMandateCount)
		{
			ConsoleHelper.WriteSectionCaption("Партии и коалиции, които получават допълнителен мандат по чл. 16, ал. 5 и 6");
			int remainingMandateCount = totalMandateCount - GetAssignedNationalMandateCount();
			while (remainingMandateCount > 0)
			{
				// Определят се партията/партиите с най-голям остатък, неудовлетворен с допълнителен мандат
				decimal maxRemainder = -1.0m;
				var partyIds = new List<int>();
				foreach (KeyValuePair<int, decimal> remainder in _nationalRemainders)
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

				if (partyIds.Count > remainingMandateCount)
				{
					// Ако броят на определените партии е по-голям от броя на оставащите мандати
					// за разпределяне, то партиите, които получават допълнителни мандати се
					// определят чрез заредените резлтати от жребии (чл. 16, ал. 6)
					Console.WriteLine(
						"Достигнат жребий при разпределяне на мандатите на партии и коалиции по чл. 16, ал. 6. Трябва да бъдат изтеглени {0} партии.",
						remainingMandateCount);
					Console.WriteLine();

					var chosenPartyIds = new List<int>();
					for (int i = 0; i < remainingMandateCount; i++)
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

				// Определените партии получават по един допълнителен мандат и не участват в
				// по-нататъчното разпределение на допълнителни мандати
				foreach (int partyId in partyIds)
				{
					Party party = Parties[partyId];
					party.NationalMandateCount++;
					_nationalRemainders.Remove(partyId);

					ConsoleHelper.WriteObject(party,
						string.Format("Общ брой получени мандати: {0}", party.NationalMandateCount));
				}

				remainingMandateCount = totalMandateCount - GetAssignedNationalMandateCount();
			}
		}

		/// <summary>
		/// Изчислява общия брой мандати, разпределени на партии и коалиции на
		/// национално ниво.
		/// </summary>
		/// <returns>Общ брой мандати, разпределени на национално ниво.</returns>
		private int GetAssignedNationalMandateCount()
		{
			return Parties.Values.Sum(p => p.NationalMandateCount);
		}

		/// <summary>
		/// Изчислява първоначалното разпределение на мандати на партии и
		/// коалиции във всеки многомандатен изборен район (по чл. 21).
		/// </summary>
		private void CalculateInitialRegionalMandates()
		{
			foreach (Region region in Regions.Values)
			{
				// Пренебрегват се МИР без мандати за разпределяне
				if (region.MandateCount == 0)
					continue;

				// Изчислява се общият брой действителни гласове, събрани от партиите,
				// определени в чл. 14, в многомандатния изборен район, както и квотата на Хеър
				ConsoleHelper.WriteSectionCaption(string.Format("Разпределение на мандатите на партии и коалиции в МИР {0} (по чл. 21)", region.RegionId));
				long totalVoteCount = region.GetTotalVoteCount();
				Console.WriteLine("Брой събрани гласове в МИР:         {0}", totalVoteCount);
				Console.WriteLine("Брой мандати за разпределяне в МИР: {0}", region.MandateCount);
				decimal hareQuota = (decimal)totalVoteCount / region.MandateCount;
				Console.WriteLine("Квота на Хеър:                      {0:F6}", hareQuota);
				Console.WriteLine();

				// Разпределят се основните мандати по чл. 21, ал. 3
				AssignBaseRegionalMandates(region, hareQuota);

				// Разпределят се допълнителните мандати по чл. 21, ал. 5 и 6
				AssignAdditionalRegionalMandates(region);
			}
		}

		/// <summary>
		/// Извършва разпределение на основните мандати на партии и коалиции в
		/// един многомандатен изборен район (по чл. 21, ал. 3).
		/// </summary>
		/// <param name="region">Многомандатният изборен район, в който да
		/// бъдат разпределени мандатите.</param>
		/// <param name="hareQuota">Предварително изчислената квота на
		/// Хеър.</param>
		private void AssignBaseRegionalMandates(Region region, decimal hareQuota)
		{
			ConsoleHelper.WriteSectionCaption("Партии и коалиции, които получават мандати по чл. 21, ал. 3");
			foreach (Party party in Parties.Values)
			{
				long partyVoteCount = region.GetVoteCount(party.PartyId);
				decimal partyQuotient = partyVoteCount / hareQuota;
				int baseMandateCount = (int)partyQuotient;

				var assignment = new MandateAssignment
				{
					RegionId = region.RegionId,
					PartyId = party.PartyId,
					BaseMandateCount = baseMandateCount,
					Remainder = partyQuotient - baseMandateCount,
				};

				party.MandateAssignments.Add(assignment);
				region.MandateAssignments.Add(party.PartyId, assignment);

				ConsoleHelper.WriteObject(assignment);
			}
		}

		/// <summary>
		/// Извършва разпределение на допълнителните мандати на партии и
		/// коалиции в един многомандатен изборен район (по чл. 21, ал. 5 и 6).
		/// </summary>
		/// <param name="region">Многомандатният изборен район, в който да
		/// бъдат разпределени мандатите.</param>
		private void AssignAdditionalRegionalMandates(Region region)
		{
			ConsoleHelper.WriteSectionCaption("Партии и коалиции, които получават допълнителен мандат по чл. 21, ал. 5 и 6");
			int assignedMandateCount = region.GetAssignedMandateCount();
			while (assignedMandateCount < region.MandateCount)
			{
				// Определят се партията/партиите с най-голям остатък, неудовлетворен с допълнителен мандат
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

				int remainingMandateCount = region.MandateCount - assignedMandateCount;
				if (partyIds.Count > remainingMandateCount)
				{
					// Ако броят на определените партии е по-голям от броя на оставащите мандати
					// за разпределяне, то допълнтелните мандати се разпределят по ред на нарастване
					// поредния номер на партията или коалицията (чл. 21, ал. 6)
					Console.WriteLine(
						"Достигнат жребий при разпределяне на мандатите на партии и коалиции по чл. 21, ал. 6. Мандатите ще бъдат получени от първите {0} партии с най-малки номера.",
						remainingMandateCount);
					Console.WriteLine();

					partyIds.Sort();
					partyIds.RemoveRange(remainingMandateCount, partyIds.Count - remainingMandateCount);
				}

				// Определените партии получават по един допълнителен мандат и не участват в
				// по-нататъчното разпределение на допълнителни мандати
				foreach (int partyId in partyIds)
				{
					MandateAssignment assignment = region.MandateAssignments[partyId];
					assignment.AdditionalMandate = true;

					ConsoleHelper.WriteObject(assignment);
				}

				assignedMandateCount = region.GetAssignedMandateCount();
			}
		}

		/// <summary>
		/// Изчислява преразпредлението на допълнителните мандати на партии и
		/// коалиции в многомандатните изборни райони (по чл. 22 - 27).
		/// </summary>
		private void CalculateAdjustedRegionalMandates()
		{
			ConsoleHelper.WriteSectionCaption("Преразпределяне на допълнителните мандати на партии и коалиции в многомандатните избирателни райони (по чл. 22 - 27)");

			HashSet<int> overassignedPartyIds;
			var ignoredRegionIds = new HashSet<int>();

			while (FindAssignmentsToAdjust(out overassignedPartyIds))
			{
				MandateAssignment minAssignment;
				Region region;

				// Намира се най-малкият остатък, за който е разпределен допълнителен мандат (чл. 24 - 26)
				do
				{
					minAssignment = FindMinAdditionalMandateAssignment(overassignedPartyIds, ignoredRegionIds);

					if (minAssignment == null)
						throw new AmbiguityException("Не съществува остатък, който да удовлетворява условията по чл. 24.");

					Console.WriteLine("Намерен разпределен допълнителен мандат за най-малък остатък:");
					Console.WriteLine("Номер на МИР:             {0}", minAssignment.RegionId);
					ConsoleHelper.WriteObject(minAssignment);

					region = Regions[minAssignment.RegionId];

					// Ако не е намерена партия, на която може да бъде преразпределен допълнителен мандат
					// в този МИР, районът се изключва от преразпределянето (по чл. 26)
					if (!region.CheckCanAssignAdditionalMandate())
					{
						ignoredRegionIds.Add(region.RegionId);
						region = null;

						Console.WriteLine("Няма възможност за преразпределяне на мандатите в този МИР. Изключва се районът от по-нататъчното преразпределяне (по чл. 26).");
						Console.WriteLine();
					}
				}
				while (region == null);

				// Намира се следващия по големина остатък, за който може да бъде разпределен допълнителен мандат (чл. 27)
				MandateAssignment eligibleAssignment = region.FindEligibleMandateAssignment();

				// Допълнителния мандат се преразпределя и партията, на която е бил разпределен преди,
				// се изключва от по-нататъчно участие в преразпределението (чл. 27)
				minAssignment.AdditionalMandate = false;
				minAssignment.AlreadyAdjusted = true;
				eligibleAssignment.AdditionalMandate = true;
				Console.WriteLine("Намерен следващ най-голям остатък, на който бива разпределен допълнителен мандат в МИР:");
				ConsoleHelper.WriteObject(eligibleAssignment);
			}
		}

		/// <summary>
		/// Намира всички партии, за които се налага да се направи
		/// преразпределение на допълнителните мандати в многомандатните
		/// изборни райони (по чл. 22 - 27).
		/// </summary>
		/// <param name="overassignedPartyIds">Колекция от номерата на всички
		/// партии, получили повече от определения на национално ниво брой
		/// мандати.</param>
		/// <returns>Истина, ако са открити партии, за които се налага
		/// преразпределение на допълнителни мандати в многомандатните изборни
		/// райони.</returns>
		private bool FindAssignmentsToAdjust(out HashSet<int> overassignedPartyIds)
		{
			overassignedPartyIds = new HashSet<int>();

			foreach (Party party in Parties.Values)
			{
				if (party.GetAssignedMandateCount() > party.NationalMandateCount)
					overassignedPartyIds.Add(party.PartyId);
			}

			return overassignedPartyIds.Count > 0;
		}

		/// <summary>
		/// Намира партията с най-малък остатък по чл. 21, ал. 2, на която е
		/// разпределен допълнителен мандат в произволен многомандатен изборен
		/// район и от която може да бъде отнет този допълнителен мандат по
		/// чл. 24.
		/// </summary>
		/// <param name="partyIds">Колекция от номерата на всички партии, на
		/// които може да бъде отнет допълнителен мандат по чл. 24.</param>
		/// <param name="ignoredRegionIds">Колекция от номерата на
		/// многомандатните изборни райони, които са изключени по
		/// чл. 26.</param>
		/// <returns>Обектът с информация за разпределените на откритата партия
		/// мандати в открития многомандатен изборен район.</returns>
		private MandateAssignment FindMinAdditionalMandateAssignment(HashSet<int> partyIds, HashSet<int> ignoredRegionIds)
		{
			decimal minRemainder = 1.0m;
			MandateAssignment minAssignment = null;
			foreach (int partyId in partyIds)
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

					// Пренебрегват се многомандатните изборни райони, които са изключени по чл. 26
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
						// Ако има повече от една партия с минимален остатък по чл. 21, ал. 2,
						// се взема партията с най-малък номер (чл. 25) 
						minAssignment = assignment;
					}
				}
			}

			return minAssignment;
		}

		/// <summary>
		/// Попълва резултатите от пропорционалното разпределение на мандати в
		/// общата колекция от резултати от цялостното разпределение.
		/// </summary>
		private void FillResults()
		{
			foreach (Region region in Regions.Values)
			{
				foreach (MandateAssignment assignment in region.MandateAssignments.Values)
				{
					// Пренебрегват се партиите, на които не са разпределени мандати в многомандатния изборен район
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

		/// <summary>
		/// Премахва кандидати от колекцията от кандидати по подадено условие.
		/// </summary>
		/// <param name="predicate">Условие, на което трябва да отговарят
		/// кандидатите, които да бъдат премахнати.</param>
		private void RemoveCandidates(Func<Candidate, bool> predicate)
		{
			_candidates = Candidates.Where(c => !predicate(c)).ToList();
		}

		/// <summary>
		/// Премахва групи от действителни гласове от колекцията от групи от
		/// действителни гласове по подадено условие. 
		/// </summary>
		/// <param name="predicate">Условие, на което трябва да отговарят
		/// групите от действително гласове, които да бъдат премахнати.</param>
		private void RemoveVoteBatches(Func<VoteBatch, bool> predicate)
		{
			_voteBatches = VoteBatches.Where(vb => !predicate(vb)).ToList();
		}
	}
}
