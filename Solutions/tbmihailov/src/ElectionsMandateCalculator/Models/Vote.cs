using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionsMandateCalculator.Models
{
    /// <summary>
    /// Vote class from Voes.txt
    /// </summary>
    public class Vote
    {
        public Vote(int mirId, int partyId, int count)
        {
            MirId = mirId;
            PartyId = partyId;
            Count = count;
        }

        public int MirId { get; set; }
        public int PartyId { get; set; }
        /// <summary>
        /// Valid votes
        /// </summary>
        public int Count { get; set; }

        #region Equals (for unit testing)
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Vote);
        }

        public bool Equals(Vote otherObj)
        {
            return otherObj.MirId == this.MirId
                    && otherObj.PartyId == this.PartyId
                    && otherObj.Count == this.Count;
        }
        #endregion
    }
}
