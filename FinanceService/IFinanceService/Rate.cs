using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.FinanceService
{
    public class Rate : ISerializable
    {
        public int Id;
        public RateType Type;
        public string Name;
        public string Description;
        public string Config;

        public Rate() { }

        public Rate(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry v in info)
                switch (v.Name)
                {
                    case "Id":
                        Id = info.GetInt32("Id"); break;
                    case "TypeId":
                        Type = (RateType)info.GetInt32("TypeId"); break;
                    case "Name":
                        Name = info.GetString("Name"); break;
                    case "Description":
                        Description = info.GetString("Description"); break;
                    case "Config":
                        Config = info.GetString("Config"); break;
                }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id, typeof(int));
            info.AddValue("TypeId", (int)Type, typeof(int));
            info.AddValue("Name", Name, typeof(string));
            info.AddValue("Description", Description, typeof(string));
            info.AddValue("Config", Config, typeof(string));
        }

        public override string ToString()
        {
            return string.Format("Id:{0}/Type:{1}/Name:{2}", Id, Type, Name);
        }
    }
}
