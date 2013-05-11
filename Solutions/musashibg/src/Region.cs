using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MandateCalculator
{
	/// <summary>
	/// Съхранява информацията за един многомандатен изборен район, зареден от
	/// MIRs.txt.
	/// </summary>
	public class Region
	{
		public Region()
		{
			Candidates = new List<Candidate>();
			VoteBatches = new Dictionary<int, VoteBatch>();
			MandateAssignments = new Dictionary<int, MandateAssignment>();
		}

		/// <summary>
		/// Номер на многомандатния изборен район.
		/// </summary>
		public int RegionId { get; set; }

		/// <summary>
		/// Име на многомандатния изборен район.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Брой мандати за разпределение в многомандатния изборен район.
		/// </summary>
		public int MandateCount { get; set; }

		/// <summary>
		/// Колекция от всички кандидати, регистрирани в многомандатния изборен
		/// район.
		/// </summary>
		public List<Candidate> Candidates { get; private set; }

		/// <summary>
		/// Колекция от всички групи от действителни гласове, подадени за
		/// всички партии, коалиции и независими кандидати в многомандатния
		/// изборен район. Ключът на колекцията е номерът на партията,
		/// коалицията или инциативния комитет, за който са подадени гласовете.
		/// </summary>
		public Dictionary<int, VoteBatch> VoteBatches { get; private set; }

		/// <summary>
		/// Колекция от обекти с информация за разпределените на всяка
		/// партия/коалиция мандати в многомандатния изборен район
		/// (по чл. 21 - 78).
		/// </summary>
		public Dictionary<int, MandateAssignment> MandateAssignments { get; private set; }

		/// <summary>
		/// Връща символен низ с информацията за многомандатни изборен район
		/// във вид, удобен за отпечатване.
		/// </summary>
		/// <returns>Символен низ с информацията за многомандатния изборен
		/// район.</returns>
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Номер на МИР: {0}", RegionId);
			builder.AppendLine();
			builder.AppendFormat("Име на МИР:   {0}", Name);
			builder.AppendLine();
			builder.AppendFormat("Брой мандати: {0}", MandateCount);
			return builder.ToString();
		}

		/// <summary>
		/// Изчислява общия брой действителни гласове, подадени за всички
		/// партии, коалиции и независими кандидати в многомандатния изборен
		/// район.
		/// </summary>
		/// <returns>Общ брой подадени действителни гласове.</returns>
		public long GetTotalVoteCount()
		{
			return VoteBatches.Values.Sum(vb => vb.VoteCount);
		}

		/// <summary>
		/// Изчислява районната квота, необходима за разпределянето на мандат
		/// на независим кандидат (по чл. 9).
		/// </summary>
		/// <returns>Районната квота за многомандатния изборен район.</returns>
		public decimal GetRegionQuota()
		{
			long totalVoteCount = GetTotalVoteCount();
			return (decimal)totalVoteCount / MandateCount;
		}

		/// <summary>
		/// Връща броя действителни гласове подадени за някоя партия, коалиция
		/// или независим кандидат многомандатния изборен район.
		/// </summary>
		/// <param name="partyId">Номер на партия, коалиция или инициативен
		/// комитет.</param>
		/// <returns>Брой действителни гласове, подадени за партията,
		/// коалицията или независимия кандидат.</returns>
		public long GetVoteCount(int partyId)
		{
			if (VoteBatches.ContainsKey(partyId))
				return VoteBatches[partyId].VoteCount;
			else
				return 0;
		}

		/// <summary>
		/// Изчислява общия брой мандати, текущо разпределени на всички партии
		/// и коалиции в многомандатния изборен район.
		/// </summary>
		/// <returns>Общ брой мандати, разпределени в многомандатния изборен
		/// район.</returns>
		public int GetAssignedMandateCount()
		{
			int mandateCount = 0;
			foreach (MandateAssignment assignment in MandateAssignments.Values)
			{
				mandateCount += assignment.BaseMandateCount;
				if (assignment.AdditionalMandate)
					mandateCount++;
			}
			return mandateCount;
		}

		/// <summary>
		/// Проверява дали в многомандатния изборен район има партия или
		/// коалиция, на която може да бъде разпределен допълнителен мандат в
		/// многомандатния изборен район (по чл. 27).
		/// </summary>
		/// <param name="partyIds">Колекция от номера на партии и коалиции,
		/// на които трябва да бъдат разпределени един или повече допълнителни
		/// мандати.</param>
		/// <returns>Истина, ако на поне една от подадените партии и коалиции
		/// може да бъде разпределен допълнителен мандат в многомандатния
		/// изборен район.</returns>
		public bool CheckCanAssignAdditionalMandate(HashSet<int> partyIds)
		{
			return MandateAssignments
				.Any(kv => partyIds.Contains(kv.Key) && !kv.Value.AdditionalMandate);
		}

		/// <summary>
		/// Намира партия или коалиция, на която може да бъде разпределен
		/// допълнителен мандат в многомандатния изборен район (по чл. 27).
		/// </summary>
		/// <returns>Обект с информацията за разпределените до момента мандати
		/// на съответната партия/коалиция в многомандатния изборен
		/// район.</returns>
		public MandateAssignment FindEligibleMandateAssignment()
		{
			decimal maxRemainder = -1.0m;
			var eligiblePartyIds = new List<int>();
			foreach (MandateAssignment assignment in MandateAssignments.Values)
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

			return MandateAssignments[eligiblePartyIds[0]];
		}
	}
}
