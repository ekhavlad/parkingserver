using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.Equipment
{
    public class DeviceState
    {
        public int DeviceId;
        public string State;
        public DateTime TimeStamp;

        public override string ToString()
        {
            return string.Format("DeviceId:{0}/TimeStamp:{1}/State:{2}", DeviceId, TimeStamp, State);
        }
    }
}
