using System.Text;

namespace MandateCalculator
{
	public class MandateAssignment
	{
		public int RegionId { get; set; }

		public int PartyId { get; set; }

		public int MainMandateCount { get; set; }

		public decimal Remainder { get; set; }

		public bool AdditionalMandate { get; set; }

		public bool AlreadyAdjusted { get; set; }

		public int TotalMandateCount
		{
			get
			{
				return (AdditionalMandate
							? MainMandateCount + 1
							: MainMandateCount);
			}
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Номер на партия/коалиция: {0}", PartyId);
			builder.AppendLine();
			builder.AppendFormat("Основен брой мандати:     {0}", MainMandateCount);
			builder.AppendLine();
			builder.AppendFormat("Остатък:                  {0:F6}", Remainder);
			builder.AppendLine();
			builder.AppendFormat("Допълнителен мандат:      {0}", (AdditionalMandate ? "да" : "не"));
			builder.AppendLine();
			builder.AppendFormat("Пълен брой мандати:       {0}",TotalMandateCount);
			return builder.ToString();
		}
	}
}
