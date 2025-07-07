using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.Equipment
{
    public class Device : ICloneable, ISerializable
    {
        public int Id;
        public int? ParentId;
        public DeviceType Type;
        public string Name;
        public string Config;

        public Device() {}

        public Device(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry v in info)
                switch (v.Name)
                {
                    case "Id":
                        Id = info.GetInt32("Id"); break;

                    case "ParentId":
                        int pId;
                        if (int.TryParse(info.GetString("ParentId"), out pId))
                            ParentId = pId;
                        else
                            ParentId = null;
                        break;

                    case "TypeId":
                        Type = (DeviceType)info.GetInt32("TypeId"); break;

                    case "Name":
                        Name = info.GetString("Name"); break;

                    case "Config":
                        Config = info.GetString("Config"); break;
                }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id, typeof(int));
            info.AddValue("ParentId", ParentId, typeof(int?));
            info.AddValue("TypeId", (int)Type, typeof(int));
            info.AddValue("Name", Name, typeof(string));
            info.AddValue("Config", Config, typeof(string));
        }

        public object Clone()
        {
            return new Device()
            {
                Id = this.Id,
                ParentId = this.ParentId,
                Type = this.Type,
                Name = this.Name,
                Config = this.Config
            };
        }

        public override string ToString()
        {
            return string.Format("Id:{0}/ParentId:{1}/Type:{2}/Name:{3}", Id, ParentId, Type, Name);
        }
    }
}
