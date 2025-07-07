using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.Equipment
{
    public class GateConfig : ISerializable
    {
        public GateType Type;
        public int? ZoneInId;
        public int? ZoneOutId;

        public GateConfig() { }

        public GateConfig(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry v in info)
                switch (v.Name)
                {
                    case "TypeId":
                        Type = (GateType)info.GetInt32("TypeId"); break;
                    case "ZoneInId":
                        ZoneInId = (int?)info.GetValue("ZoneInId", typeof(int?)); break;
                    case "ZoneOutId":
                        ZoneOutId = (int?)info.GetValue("ZoneOutId", typeof(int?)); break;
                }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TypeId", (int)Type, typeof(int));
            info.AddValue("ZoneInId", ZoneInId, typeof(int?));
            info.AddValue("ZoneOutId", ZoneOutId, typeof(int?));
        }

        public override string ToString()
        {
            return string.Format("Type:{0}/In:{1}/Out:{2}", Type, ZoneInId, ZoneOutId);
        }
    }
}
