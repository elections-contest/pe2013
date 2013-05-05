using System.Text;

namespace MandateCalculator
{
	public class Candidate
	{
		public Candidate()
		{
			IsIndependent = false;
		}

		public int RegionId { get; set; }

		public int PartyId { get; set; }

		public int CandidateId { get; set; }

		public string Name { get; set; }

		public bool IsIndependent { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Номер на МИР:                 {0}", RegionId);
			builder.AppendLine();
			builder.AppendFormat("Номер на партия/коалиция/ИК:  {0}", PartyId);
			builder.AppendLine();
			builder.AppendFormat("Номер на кандидата в листата: {0}", CandidateId);
			builder.AppendLine();
			builder.AppendFormat("Име на кандидат:              {0}", Name);
			return builder.ToString();
		}
	}
}
