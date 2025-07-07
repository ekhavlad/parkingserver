using System.Collections.Generic;

namespace Hermes.Parking.Server.ServerDictionary
{
    public interface IServerDictionaryManager
    {
        IEnumerable<ServerDictionary> GetAllDictionaries();
    }
}
