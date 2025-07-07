using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.Equipment
{
    public class DeviceEvent
    {
        public int DeviceId;
        public int TypeId;
        public string Value;
        public DateTime TimeStamp;

        public override string ToString()
        {
            return string.Format("DeviceId:{0}/TypeId:{1}/TimeStamp:{2}/State:{3}", DeviceId, TypeId, TimeStamp, Value);
        }
    }
}
