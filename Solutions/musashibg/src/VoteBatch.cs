using System.Text;

namespace MandateCalculator
{
	/// <summary>
	/// Съхранява информацията за една група от действителни гласове, заредена
	/// от Votes.txt.
	/// </summary>
	public class VoteBatch
	{
		/// <summary>
		/// Номер на многомандатния изборен район, в който са подадени
		/// гласовете.
		/// </summary>
		public int RegionId { get; set; }

		/// <summary>
		/// Номер на партия, коалиция или инициативен комитет, за който са
		/// подадени гласовете.
		/// </summary>
		public int PartyId { get; set; }

		/// <summary>
		/// Брой подадени действителни гласове за съответната партия, коалиция
		/// или инициативен комитет в този МИР.
		/// </summary>
		public long VoteCount { get; set; }

		/// <summary>
		/// Връща символен низ с информацията за групата от действителни
		/// гласове във вид, удобен за отпечатване.
		/// </summary>
		/// <returns>Символен низ с информацията за групата от действителни
		/// гласове.</returns>
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
