using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Parking.Server.SerializationService
{
    public interface ISerializationService
    {
        string Serialize(object Object);
        T Deserialize<T>(string JSON);
    }
}