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

namespace Hermes.Parking.Server.Reports.PlacesLog
{
    public partial class PlacesLogDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService db;

        public PlacesLogDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
        }

        public override string GetName()
        {
            return Constants.PLACESLOG_DATA_PROVIDER_NAME;
        }


        public override void Create(string ObjectType, object InitialObject, ref object Output)
        {
            switch (ObjectType)
            {
                case Constants.PLACESLOG_OBJECT_TYPE_NAME:
                    Output = CreateLog(InitialObject);
                    break;
            }
        }

        public override void Get(string ObjectType, IDictionary<string, object> Filter, ref object Output)
        {
            switch (ObjectType)
            {
                case Constants.PLACESLOG_OBJECT_TYPE_NAME:
                    if (Filter == null || !Filter.ContainsKey("ZoneId"))
                        throw new ServerDefinedException("При получении последней записи лога мест не указана зона");
                    Output = GetLastPlacesLogByZoneId((int)Filter["ZoneId"]);
                    break;
            }
        }

        public override void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
            switch (ObjectType)
            {
                case Constants.PLACESLOG_OBJECT_TYPE_NAME:
                    Output = GetPlacesLogs(Filter);
                    break;
            }
        }
    }
}
