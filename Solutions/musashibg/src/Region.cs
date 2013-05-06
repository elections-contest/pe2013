using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MandateCalculator
{
	public class Region
	{
		public Region()
		{
			Candidates = new List<Candidate>();
			VoteBatches = new Dictionary<int, VoteBatch>();
			MandateAssignments = new Dictionary<int, MandateAssignment>();
		}

		public int RegionId { get; set; }

		public string Name { get; set; }

		public int MandateCount { get; set; }

		public List<Candidate> Candidates { get; private set; }

		public Dictionary<int, VoteBatch> VoteBatches { get; private set; }

		public Dictionary<int, MandateAssignment> MandateAssignments { get; private set; }

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

		public int GetTotalVoteCount()
		{
			return VoteBatches.Values.Sum(vb => vb.VoteCount);
		}

		public decimal GetRegionQuota()
		{
			int totalVoteCount = GetTotalVoteCount();
			return (decimal)totalVoteCount / MandateCount;
		}

		public int GetVoteCount(int partyId)
		{
			if (VoteBatches.ContainsKey(partyId))
				return VoteBatches[partyId].VoteCount;
			else
				return 0;
		}

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

		public bool CheckCanAssignAdditionalMandate(HashSet<int> partyIds)
		{
			return MandateAssignments
				.Any(kv => partyIds.Contains(kv.Key) && !kv.Value.AdditionalMandate);
		}

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
