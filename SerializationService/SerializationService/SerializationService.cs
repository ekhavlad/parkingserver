using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hermes.Core.Interfaces;
using Newtonsoft;

namespace Hermes.Parking.Server.SerializationService
{
    public class SerializationService : BaseService, ISerializationService
    {
        public override string GetName() { return Constants.SERIALIZATION_SERVICE_NAME; }

        public string Serialize(object Object)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Object);
            return json;
        }

        public T Deserialize<T>(string JSON)
        {
            T result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JSON);
            return result;
        }
    }
}
