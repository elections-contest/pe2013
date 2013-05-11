using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionsMandateCalculator.Models
{
    /// <summary>
    /// MIR class from MIRs.txt
    ///1;“МИР 1“;10
    ///2;“МИР 2“;5
    ///3;“Чужбина“;0
    /// </summary>
    public class Mir
    {
        public Mir(int id, string name, int mandatesLimit)
        {
            Id = id;
            MandatesLimit = mandatesLimit;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int MandatesLimit { get; set; }

        public string DisplayName { get { return string.Format("{0} - {1}", Id, Name); } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(string.Format("МИР {0}", Id));
            sb.AppendLine(string.Format("Наименование:{0}", Name));
            return sb.ToString();
        }

        #region Equals (for unit testing)
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Mir);
        }

        public bool Equals(Mir otherMir)
        {
            return otherMir.Id == this.Id
                    && otherMir.Name == this.Name
                    && otherMir.MandatesLimit == this.MandatesLimit;
        }
        #endregion
    }
}
