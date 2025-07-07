using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;

namespace Hermes.Parking.Server.ServerDictionary
{
    public class ServerDictionaryManager : BaseService, IServerDictionaryManager
    {
        private IDataService dataService;

        private IEnumerable<ServerDictionary> dictionaries;

        public override string GetName() { return Constants.SERVER_DICTIONARY_MANAGER_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            this.dataService = Context.GetService<IDataService>(Hermes.Parking.Server.DataService.Constants.DATA_SERVICE_NAME);
        }

        public override void OnStart()
        {
            dictionaries = dataService.GetList<ServerDictionary>(Constants.SERVER_DICTIONARY_OBJECT_TYPE_NAME, null).ToList();
        }

        public IEnumerable<ServerDictionary> GetAllDictionaries()
        {
            return dictionaries.Select(x => (ServerDictionary)x.Clone());
        }
    }
}
