using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using System.Threading;

namespace Hermes.Parking.Server.ServerDictionary
{
    public class ServerDictionaryDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService dbService;

        public ServerDictionaryDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.dbService = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
        }

        public override string GetName()
        {
            return Constants.SERVER_DICTIONARY_DATA_PROVIDER_NAME;
        }

        public override void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
            switch (ObjectType)
            {
                case Constants.SERVER_DICTIONARY_OBJECT_TYPE_NAME:
                    Output = GetAllDictionaries();
                    break;
            }
        }

        private IEnumerable<ServerDictionary> GetAllDictionaries()
        {
            IEnumerable<Tuple<int, int, string>> allItems = GetAllDictionaryItems();

            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Dictionary");
            DataSet sqlResult = dbService.GetDataSet(cmd);

            IList<ServerDictionary> result = new List<ServerDictionary>();

            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                int id = (int)row["Id"];
                string name = (string)row["Name"];
                IDictionary<int, string> items = allItems.Where(x => x.Item2 == id).ToDictionary(x => x.Item1, x => x.Item3);
                result.Add(new ServerDictionary(id, name, items));
            }

            return result;
        }

        private IEnumerable<Tuple<int,int, string>> GetAllDictionaryItems()
        {
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM DictionaryItem");
            DataSet sqlResult = dbService.GetDataSet(cmd);

            IList<Tuple<int, int, string>> result = new List<Tuple<int,int, string>>();

            foreach (DataRow row in sqlResult.Tables[0].Rows)
                result.Add(new Tuple<int, int, string>((int)row["Id"], (int)row["DictionaryId"], (string)row["Name"]));

            return result;
        }
    }
}
