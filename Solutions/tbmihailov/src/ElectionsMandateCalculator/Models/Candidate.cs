using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionsMandateCalculator.Models
{
    /// <summary>
    /// Model from Candidates.txt
    /// ex:
    /// 1;1;“Кандидат 1 в МИР 1 – Партия 1“
    /// 1;2;“Кандидат 2 в МИР 1 – Партия 1“
    /// 1;1000;“Независим кандидат 1 в МИР 1“
    /// 2;1001;“Независим кандидат 1 в МИР 2“
    /// </summary>
    public class Candidate
    {
        public Candidate(int mirId, int partyId, string name)
        {
            MirId = mirId;
            PartyId = partyId;
            Name = name;
            SeqNum = 0;
            PartyType = partyId >= 1000 ? PartyType.InitCommittee : PartyType.Party;
        }

        public int MirId { get; set; }
        public int PartyId { get; set; }
        public int SeqNum { get; set; }
        public string Name { get; set; }

        public PartyType PartyType { get; set; }

        #region Equals (for unit testing)
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Candidate);
        }

        public bool Equals(Candidate otherObj)
        {
            return otherObj.MirId == this.MirId
                    && otherObj.PartyId == this.PartyId
                    && otherObj.PartyType == this.PartyType
                    && otherObj.Name == this.Name;
        }
        #endregion
    }
}
