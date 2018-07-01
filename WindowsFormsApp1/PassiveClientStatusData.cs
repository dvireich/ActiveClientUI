using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class PassiveClientStatusData
    {
        public string id;
        public Status IsAlive;
        public string NickName;

        public PassiveClientStatusData(string id, Status IsAlive, string NickName)
        {
            this.id = id;
            this.IsAlive = IsAlive;
            this.NickName = NickName;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PassiveClientStatusData;
            return other != null &&
                   other.id == this.id &&
                   other.IsAlive == this.IsAlive &&
                   other.NickName == this.NickName;
        }
    }

    
}
