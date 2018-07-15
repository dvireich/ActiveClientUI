using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.DataModel;
using WindowsFormsApp1.DataModel.Enums;

namespace WindowsFormsApp1
{
    public class PassiveClientStatusData : Showable
    {
        public string id;
        public StatusType IsAlive;
        public string NickName;

        public PassiveClientStatusData(string id, StatusType isAlive, string nickName) : base(isAlive.ToString(), new List<string>() { id, nickName }, (int)isAlive)
        {
            this.id = id;
            this.IsAlive = isAlive;
            this.NickName = nickName;
        }

        public override bool Equals(object obj)
        {
            return obj is PassiveClientStatusData other &&
                   other.id == this.id &&
                   other.IsAlive == this.IsAlive &&
                   other.NickName == this.NickName;
        }
    }
}
