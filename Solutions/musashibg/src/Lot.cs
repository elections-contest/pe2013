namespace MandateCalculator
{
	/// <summary>
	/// Съхранява информацията за един изтеглен жребий, зареден от Lots.txt.
	/// </summary>
	public class Lot
	{
		/// <summary>
		/// Номер на партията или коалицията, която е била изтеглена при
		/// жребия.
		/// </summary>
		public int PartyId { get; set; }

		/// <summary>
		/// Връща символен низ с информацията за изтегления жребий във вид,
		/// удобен за отпечатване.
		/// </summary>
		/// <returns>Символен низ с информацията за изтегления
		/// жребий.</returns>
		public override string ToString()
		{
			return string.Format("Номер на изтеглена партия/коалиция: {0}", PartyId);
		}
	}
}
