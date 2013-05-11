using System.Text;

namespace MandateCalculator
{
	/// <summary>
	/// Съхранява информация за разпределените на конкретна партия или коалиция
	/// мандати в многомандатен изборен район (по чл. 21 - 27).
	/// </summary>
	public class MandateAssignment
	{
		/// <summary>
		/// Номер на многомандатния изборен район, в който са разпределени
		/// мандатите.
		/// </summary>
		public int RegionId { get; set; }

		/// <summary>
		/// Номер на партия или коалиция, на които са разпределени мандатите.
		/// </summary>
		public int PartyId { get; set; }

		/// <summary>
		/// Основен брой мандати, разпределени на партията/коалицията
		/// (по чл. 21, ал. 3).
		/// </summary>
		public int BaseMandateCount { get; set; }

		/// <summary>
		/// Остатък от делението по чл. 21, ал. 2.
		/// </summary>
		public decimal Remainder { get; set; }

		/// <summary>
		/// Указва дали на партията/коалицията е разпределен допълнителен
		/// мандат (по чл. 21, ал 5 или 6 или по чл. 27).
		/// </summary>
		public bool AdditionalMandate { get; set; }

		/// <summary>
		/// Указва дали на партията/коалицията вече е бил разпределен и отнет
		/// допълнителен мандат (по чл. 27).
		/// </summary>
		public bool AlreadyAdjusted { get; set; }

		/// <summary>
		/// Общ брой мандати, разпределени на партията/коалицията в
		/// многомандатния изборен район.
		/// </summary>
		public int TotalMandateCount
		{
			get
			{
				return (AdditionalMandate
							? BaseMandateCount + 1
							: BaseMandateCount);
			}
		}

		/// <summary>
		/// Връща символен низ с информацията за разпределените мандати във
		/// вид, удобен за отпечатване.
		/// </summary>
		/// <returns>Символен низ с информацията за разпределените
		/// мандати.</returns>
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Номер на партия/коалиция: {0}", PartyId);
			builder.AppendLine();
			builder.AppendFormat("Основен брой мандати:     {0}", BaseMandateCount);
			builder.AppendLine();
			builder.AppendFormat("Остатък:                  {0:F6}", Remainder);
			builder.AppendLine();
			builder.AppendFormat("Допълнителен мандат:      {0}", (AdditionalMandate ? "да" : "не"));
			builder.AppendLine();
			builder.AppendFormat("Пълен брой мандати:       {0}", TotalMandateCount);
			return builder.ToString();
		}
	}
}
