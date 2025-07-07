using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.Reports.PassageLog
{
    public class PassageLog
    {
        public long Id;
        public int GateId;
        public DateTime InsertStamp;
    }
}
