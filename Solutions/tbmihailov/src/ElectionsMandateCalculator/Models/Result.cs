using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionsMandateCalculator.Models
{
    /// <summary>
    /// Result info from Result.txt
    /// Example:5;3;2
    /// 5;4;1
    /// 6;1;4
    /// 6;2;4
    /// </summary>
    public class Result
    {
        public Result(int mirId, int partyId, int mandatesCount)
        {
            this.MirId = mirId;
            this.PartyId = partyId;
            this.MandatesCount = mandatesCount;
        }

        public Result()
        {

        }
        public int MirId { get; set; }
        public int PartyId { get; set; }
        public int MandatesCount { get; set; }

        #region Equals (for unit testing)
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Result);
        }

        public bool Equals(Result otherObj)
        {
            return otherObj.MirId == this.MirId
                    && otherObj.PartyId == this.PartyId
                    && otherObj.MandatesCount == this.MandatesCount;
        }
        #endregion

    }
}
