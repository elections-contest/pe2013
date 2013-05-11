using System.Text;

namespace MandateCalculator
{
	/// <summary>
	/// Съхранява информацията за един ред от изходния файл Result.txt.
	/// </summary>
	public class Result
	{
		/// <summary>
		/// Номер на многомандатен изборен район.
		/// </summary>
		public int RegionId { get; set; }

		/// <summary>
		/// Номер на партия, коалиция или инициативен комитет.
		/// </summary>
		public int PartyId { get; set; }

		/// <summary>
		/// Брой мандати, разпределени на партията, коалицията или независимия
		/// кандидат в многомандатния изборен район.
		/// </summary>
		public int MandateCount { get; set; }

		/// <summary>
		/// Връща символен низ с информацията за окончателно разпределените
		/// мандати във вид, удобен за отпечатване.
		/// </summary>
		/// <returns>Символен низ с информацията за разпределените
		/// мандати.</returns>
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("Номер на МИР:                {0}", RegionId);
			builder.AppendLine();
			builder.AppendFormat("Номер на партия/коалиция/ИК: {0}", PartyId);
			builder.AppendLine();
			builder.AppendFormat("Брой спечелени мандати:      {0}", MandateCount);
			return builder.ToString();
		}
	}
}
