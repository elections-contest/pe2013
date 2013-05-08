using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MandateCalculator
{
	/// <summary>
	/// Съхранява информацията за една партия/коалиция, заредени от
	/// Parties.txt.
	/// </summary>
	public class Party
	{
		public Party()
		{
			Candidates = new List<Candidate>();
			VoteBatches = new Dictionary<int, VoteBatch>();
			MandateAssignments = new List<MandateAssignment>();
		}

		/// <summary>
		/// Номер на партията/коалицията.
		/// </summary>
		public int PartyId { get; set; }

		/// <summary>
		/// Име на партията/коалицията.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Колекция от всички кандидати, регистрирани в кандидатската листа на
		/// партията/коалицията.
		/// </summary>
		public List<Candidate> Candidates { get; private set; }

		/// <summary>
		/// Колекция от всички групи от действителни гласове, подадени във
		/// всеки многомандатен изборен район за партията/коалицията. Ключът на
		/// колекцията е номерът на многомандатния изборен район, в който са
		/// подадени гласовете.
		/// </summary>
		public Dictionary<int, VoteBatch> VoteBatches { get; private set; }

		/// <summary>
		/// Брой мандати, които партията/коалицията трябва да получи на
		/// национално ниво (по чл. 16).
		/// </summary>
		public int NationalMandateCount { get; set; }

		/// <summary>
		/// Колекция от обекти с информация за разпределените на
		/// партията/коалицията мандати във всеки многомандатен изборен район
		/// (по чл. 21 - 27).
		/// </summary>
		public List<MandateAssignment> MandateAssignments { get; private set; }

		/// <summary>
		/// Връща символен низ с информацията за партията/коалицията във вид,
		/// удобен за отпечатване.
		/// </summary>
		/// <returns>Символен низ с информацията за
		/// партията/коалицията.</returns>
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Номер на партия/коалиция: {0}", PartyId);
			builder.AppendLine();
			builder.AppendFormat("Име на партия/коалиция:   {0}", Name);
			return builder.ToString();
		}

		/// <summary>
		/// Изчислява общия брой действителни гласове, подадени за
		/// партията/коалицията във всички многомандатни изборни райони.
		/// </summary>
		/// <returns>Общ брой подадени действителни гласове.</returns>
		public long GetTotalVoteCount()
		{
			return VoteBatches.Values.Sum(vb => vb.VoteCount);
		}

		/// <summary>
		/// Изчислява общия брой мандати, текущо разпределени на
		/// партията/коалицията в многомандатните изборни райони.
		/// </summary>
		/// <returns>Общ брой мандати, разпределени на регионално
		/// ниво.</returns>
		public int GetAssignedMandateCount()
		{
			int mandateCount = 0;
			foreach (MandateAssignment assignment in MandateAssignments)
			{
				mandateCount += assignment.BaseMandateCount;
				if (assignment.AdditionalMandate)
					mandateCount++;
			}
			return mandateCount;
		}
	}
}
