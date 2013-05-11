using System.Text;

namespace MandateCalculator
{
	/// <summary>
	/// Съхранява информацията за един кандидат, зареден от Candidates.txt.
	/// </summary>
	public class Candidate
	{
		public Candidate()
		{
			IsIndependent = false;
		}

		/// <summary>
		/// Номер на многомандатния изборен район, в който е регистриран
		/// кандидатът.
		/// </summary>
		public int RegionId { get; set; }

		/// <summary>
		/// Номер на партия, коалиция или инициативен комитет, от който е
		/// издигнат кандидатът.
		/// </summary>
		public int PartyId { get; set; }

		/// <summary>
		/// Пореден номер на кандидата в кандидатската листа на партията,
		/// коалицията или инициативния комитет, от който е издигнат
		/// кандидатът.
		/// </summary>
		public int CandidateId { get; set; }

		/// <summary>
		/// Име на кандидата.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Указва дали кандидата е независим.
		/// </summary>
		public bool IsIndependent { get; set; }

		/// <summary>
		/// Връща символен низ с информацията за кандидата във вид, удобен за
		/// отпечатване.
		/// </summary>
		/// <returns>Символен низ с информацията за кандидата.</returns>
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
