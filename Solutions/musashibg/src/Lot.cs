namespace MandateCalculator
{
	public class Lot
	{
		public int PartyId { get; set; }

		public override string ToString()
		{
			return string.Format("Номер на изтеглена партия/коалиция: {0}", PartyId);
		}
	}
}
