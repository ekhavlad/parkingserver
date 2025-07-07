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

namespace Hermes.Parking.Server.Reports.PassageLog
{
    public partial class PassageLogDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService db;

        public PassageLogDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
        }

        public override string GetName()
        {
            return Constants.PASSAGELOG_DATA_PROVIDER_NAME;
        }


        public override void Create(string ObjectType, object InitialObject, ref object Output)
        {
            switch (ObjectType)
            {
                case Constants.PASSAGELOG_OBJECT_TYPE_NAME:
                    Output = CreateLog(InitialObject);
                    break;
            }
        }

        public override void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
            switch (ObjectType)
            {
                case Constants.PASSAGELOG_OBJECT_TYPE_NAME:
                    Output = GetPassageLogs(Filter);
                    break;
            }
        }
    }
}
