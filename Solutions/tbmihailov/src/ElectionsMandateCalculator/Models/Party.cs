using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionsMandateCalculator.Models
{
    /// <summary>
    /// Party from Parties.txt
    /// 1;“Партия 1“
    /// 2;“Коалиция 1“
    /// 1000;“Инициативен комитет в МИР 1“
    /// 1001;“Инициативен комитет в МИР 2“
    /// </summary>
    public class Party
    {
        public Party(int id, string name)
        {
            Id = id;
            Name = name;
            Type = id >= 1000 ? PartyType.InitCommittee : PartyType.Party;
            MandatesCount = 0;
        }

        public string DisplayName
        {
            get
            {
                return string.Format("{0} - {1}", Id, Name);
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public PartyType Type { get; set; }
        public int MandatesCount { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(string.Format("Партия {0}", Id));
            sb.AppendLine(string.Format("Име:{0}", Name));
            sb.AppendLine(string.Format("Тип:{0}", Type == PartyType.InitCommittee ? "Инициативен комитет" : "Партия/Коалиция"));
            return sb.ToString();
        }

        #region Equals (for unit testing)
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Party);
        }

        public bool Equals(Party otherObj)
        {
            return otherObj.Id == this.Id
                    && otherObj.Name == this.Name
                    && otherObj.MandatesCount == this.MandatesCount
                    && otherObj.Type == this.Type;
        }
        #endregion
    }
}
