using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class ServerRequest : ICloneable, ISerializable
    {
        public int Id;
        public RequestGroup Group;
        public string Name;
        public string Description;

        public ServerRequest() { }

        public ServerRequest(SerializationInfo info, StreamingContext context)
        {
            Id = info.GetInt32("Id");
            Group = (RequestGroup)info.GetInt32("GroupId");
            Name = info.GetString("Name");
            Description = info.GetString("Description");
        }

        public object Clone()
        {
            return new ServerRequest() { Id = this.Id, Group = this.Group, Name = this.Name, Description = this.Description };
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id, typeof(int));
            info.AddValue("GroupId", (int)Group, typeof(int));
            info.AddValue("Name", Name, typeof(string));
            info.AddValue("Description", Description, typeof(string));
        }
    }
}
