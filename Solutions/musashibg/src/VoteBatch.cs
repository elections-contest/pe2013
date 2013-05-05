using System.Text;

namespace MandateCalculator
{
	public class VoteBatch
	{
		public int RegionId { get; set; }

		public int PartyId { get; set; }

		public int VoteCount { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Номер на МИР:                  {0}", RegionId);
			builder.AppendLine();
			builder.AppendFormat("Номер на партия/коалиция/ИК:   {0}", PartyId);
			builder.AppendLine();
			builder.AppendFormat("Получени действителни гласове: {0}", VoteCount);
			return builder.ToString();
		}
	}
}
