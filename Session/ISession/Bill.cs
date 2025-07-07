using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Parking.Server.Session
{
    public class Bill
    {
        public long Id;
        public long SessionId;
        public int Summ;
        public DateTime CreateTime;

        public override string ToString()
        {
            return string.Format("Id:{0}/Session:{1}/Summ:{2}/Date:{3:yyyy-MM-ddTHH:mm:ss}", Id, SessionId, Summ, CreateTime);
        }
    }
}
