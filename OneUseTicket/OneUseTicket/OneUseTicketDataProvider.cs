using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using Hermes.Parking.Server.Session;

namespace Hermes.Parking.Server.OneUseTicket
{
    public class OneUseTicketDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService db;

        public OneUseTicketDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
        }

        public override string GetName()
        {
            return Constants.ONE_USE_TICKET_DATA_PROVIDER_NAME;
        }

        public override void Get(string ObjectType, IDictionary<string, object> Filter, ref object Output)
        {
            string sql = string.Format(@"
                DECLARE @number bigint
                SELECT TOP 1 @number = [Number]
                FROM OneUseTicket AS t
                LEFT JOIN [Session] AS s ON s.TicketNumber = t.Number AND s.StatusId IN (2, 5, 6)
                WHERE s.Id IS NULL
                ORDER BY t.LastUseDate

                UPDATE OneUseTicket SET LastUseDate = GETDATE() WHERE Number = @number

                SELECT @number AS Number",
                    string.Join(",", Enum.GetValues(typeof(SessionStatus)).Cast<int>().ToArray().Except(Enum.GetValues(typeof(ActiveSessionStatus)).Cast<int>().ToArray()))
                    );

            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);

            if (sqlResult.Tables[0].Rows.Count == 0)
                throw new ServerDefinedException("Нет доступных билетов");
            else
                Output = (long)sqlResult.Tables[0].Rows[0]["Number"];
        }
    }
}
