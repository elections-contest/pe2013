using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionsMandateCalculator.Models
{
    public class Lot
    {
        public Lot(int partyId)
        {
            this.PartyId = partyId;
        }
        public int PartyId { get; private set; }
    }
}
