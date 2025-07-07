using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.Equipment
{
    public class ZoneConfig : ISerializable
    {
        public int PlacesNumber;
        public int? RateId;

        public ZoneConfig(){}

        public ZoneConfig(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry v in info)
                switch (v.Name)
                {
                    case "PlacesNumber":
                        PlacesNumber = info.GetInt32("PlacesNumber"); break;

                    case "RateId":
                        RateId = (int?)info.GetValue("RateId", typeof(int?)); break;
                }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("PlacesNumber", PlacesNumber, typeof(int));
            info.AddValue("RateId", RateId, typeof(int?));
        }

        public override string ToString()
        {
            return string.Format("RateId:{0}/Places:{1}", RateId, PlacesNumber);
        }
    }
}
