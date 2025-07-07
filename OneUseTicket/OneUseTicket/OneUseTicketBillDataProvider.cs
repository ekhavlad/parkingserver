using System;
using System.Collections.Generic;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataBaseService;
using Hermes.Parking.Server.DataService;
using Hermes.Parking.Server.FinanceService;

namespace Hermes.Parking.Server.OneUseTicket
{
    public class OneUseTicketBillDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService db;
        private IDataService ds;
        private IFinanceService fs;

        public OneUseTicketBillDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.ds = Context.GetService<IDataService>(Hermes.Parking.Server.DataService.Constants.DEFAULT_DATA_SERVICE_NAME);
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataBaseService.Constants.DEFAULT_DATABASE_SERVICE_NAME);
            this.fs = Context.GetService<IFinanceService>(Hermes.Parking.Server.FinanceService.Constants.DEFAULT_FINANCE_SERVICE_NAME);
        }

        public override string GetName()
        {
            return Constants.DEFAULT_ONE_USE_TICKET_BILL_DATA_PROVIDER_NAME;
        }

        public override T CreateObject<T>(IDictionary<string, object> InitialData)
        {
            if (!InitialData.ContainsKey("OneUseTicket") || !(InitialData["OneUseTicket"] is IOneUseTicket))
                throw new InvalidCastException("Not 'IOneUseTicket' object given");

            if (!InitialData.ContainsKey("Bill") || !(InitialData["Bill"] is IBill))
                throw new InvalidCastException("Not 'IBill' object given");

            IBill bill = (IBill)InitialData["Bill"];
            IOneUseTicket ticket = (IOneUseTicket)InitialData["OneUseTicket"];

            string sql = "INSERT INTO OneUseTicketBill (BillId, OneUseTicketId) VALUES (@BillId, @TicketId)";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@BillId", SqlDbType.BigInt).Value = bill.Id;
                    cmd.Parameters.Add("@TicketId", SqlDbType.BigInt).Value = ticket.Id;
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return (T)(object)bill;
                }
            }
        }

        public override IEnumerable<T> GetObjects<T>(IDictionary<string, object> Filter)
        {
            if (!Filter.ContainsKey("OneUseTicket") || !(Filter["OneUseTicket"] is IOneUseTicket))
                throw new InvalidCastException("Not 'IOneUseTicket' object given");

            IOneUseTicket ticket = (IOneUseTicket)Filter["OneUseTicket"];

            string sql = "SELECT BillId FROM OneUseTicketBill WHERE OneUseTicketId = @TicketId";

            using (SqlConnection conn = db.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@TicketId", SqlDbType.BigInt).Value = ticket.Id;
                    DataTable sqlResult = new DataTable();
                    IList<long> ids = new List<long>();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(sqlResult);
                        for (int i = 0; i < sqlResult.Rows.Count; i++)
                            ids.Add((long)sqlResult.Rows[i]["BillId"]);

                        IDictionary<string, object> filter = new Dictionary<string, object>();
                        filter.Add("Ids", ids);
                        return (IEnumerable<T>)ds.GetObjects<IBill>(FinanceService.Constants.DEFAULT_BILL_DATA_PROVIDER_NAME, filter);
                    }
                }
            }
        }
    }
}
