using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class UserRole : ICloneable, ISerializable
    {
        public int Id;
        public string Name;
        public List<string> Requests;

        public UserRole() { }

        public UserRole(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry v in info)
                switch (v.Name)
                {
                    case "Id":
                        Id = info.GetInt32("Id"); break;
                    case "Name":
                        Name = info.GetString("Name"); break;
                    case "Requests":
                        Requests = (List<string>)info.GetValue("Requests", typeof(List<string>));
                        break;
                }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id, typeof(int));
            info.AddValue("Name", Name, typeof(string));
            info.AddValue("Requests", Requests, typeof(List<string>));
        }

        public object Clone()
        {
            return new UserRole()
            {
                Id = this.Id,
                Name = this.Name,
                Requests = this.Requests.ToList()
            };
        }

        public override string ToString()
        {
            return string.Format("Id:{0}/Name:{1}/Requests:[{2}]", Id, Name, (Requests == null) ? "" : string.Join(",", Requests.ToArray()));
        }
    }
}
