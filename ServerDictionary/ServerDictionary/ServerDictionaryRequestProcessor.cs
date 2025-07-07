using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Core.Interfaces;

namespace Hermes.Parking.Server.ServerDictionary
{
    public class ServerDictionaryRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private IServerDictionaryManager serverDictionaryManager;
        private Hermes.Parking.Server.SerializationService.ISerializationService serializationService;

        public ServerDictionaryRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.serverDictionaryManager = Context.GetService<IServerDictionaryManager>(Constants.SERVER_DICTIONARY_MANAGER_NAME);
            this.serializationService = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
        }

        public string GetName() { return Constants.SERVER_DICTIONARY_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "GetAllServerDictionaries":
                    return GetAllServerDictionaries(Request.Data, Response.Data);

                default:
                    return true;
            }
        }

        public bool GetAllServerDictionaries(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<ServerDictionary> d = serverDictionaryManager.GetAllDictionaries();
            OutputData["Dictionaries"] = serializationService.Serialize(d);
            return true;
        }
    }
}
