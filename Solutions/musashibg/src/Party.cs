using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MandateCalculator
{
	public class Party
	{
		public Party()
		{
			Candidates = new List<Candidate>();
			VoteBatches = new Dictionary<int, VoteBatch>();
			MandateAssignments = new List<MandateAssignment>();
		}

		public int PartyId { get; set; }

		public string Name { get; set; }

		public List<Candidate> Candidates { get; private set; }

		public Dictionary<int, VoteBatch> VoteBatches { get; private set; }

		public int NationalMandateCount { get; set; }

		public List<MandateAssignment> MandateAssignments { get; private set; }

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Номер на партия/коалиция: {0}", PartyId);
			builder.AppendLine();
			builder.AppendFormat("Име на партия/коалиция:   {0}", Name);
			return builder.ToString();
		}

		public int GetTotalVoteCount()
		{
			return VoteBatches.Values.Sum(vb => vb.VoteCount);
		}

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
