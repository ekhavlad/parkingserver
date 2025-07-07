using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class User : ICloneable, ISerializable
    {
        public int Id;
        public UserType Type;
        public string Login;
        public string Password;
        public bool IsActive;
        public List<int> Roles;

        public User(){}

        public User(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry v in info)
                switch (v.Name)
                {
                    case "Id":
                        Id = info.GetInt32("Id"); break;
                    case "TypeId":
                        Type = (UserType)info.GetInt32("TypeId"); break;
                    case "Login":
                        Login = info.GetString("Login"); break;
                    case "Password":
                        Password = info.GetString("Password"); break;
                    case "IsActive":
                        IsActive = info.GetBoolean("IsActive"); break;
                    case "Roles":
                        Roles = (List<int>)info.GetValue("Roles", typeof(List<int>));
                        break;
                }


        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id, typeof(int));
            info.AddValue("TypeId", (int)Type, typeof(int));
            info.AddValue("Login", Login, typeof(string));
            info.AddValue("IsActive", IsActive, typeof(string));
            info.AddValue("Roles", Roles, typeof(List<int>));
        }

        public object Clone()
        {
            return new User()
            {
                Id = this.Id,
                Type = this.Type,
                Login = this.Login,
                Password = this.Password,
                IsActive = this.IsActive,
                Roles = this.Roles.ToList()
            };
        }

        public override string ToString()
        {
            return string.Format("Id:{0}/Type:{1}/Login:{2}/IsActive:{3}/Roles:[{4}]", Id, Type, Login, IsActive, (Roles == null) ? "" : string.Join(",", Roles.ToArray()));
        }
    }
}
