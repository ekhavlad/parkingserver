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

namespace Hermes.Parking.Server.Session
{
    public partial class SessionDataProvider : BaseDataProvider
    {
        private IEnumerable<Bill> GetBills(IDictionary<string, object> Filter)
        {
            string sql = @"SELECT * FROM SessionBill WHERE 1 = 1";

            if (Filter != null)
            {
                if (Filter.ContainsKey("SessionId"))
                    sql += string.Format(" AND [SessionId] = {0}", (long)Filter["SessionId"]);
            }
            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);

            IList<Bill> result = new List<Bill>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                Bill bill = ParseBill(row);
                result.Add(bill);
            }
            return result;
        }

        private Bill CreateBill(object InitialObject)
        {
            Bill bill = (Bill)InitialObject;

            SqlCommand cmd = new SqlCommand(@"INSERT INTO SessionBill (SessionId, Summ, [Date]) OUTPUT inserted.* VALUES (@SessionId, @Summ, @Date);");
            cmd.Parameters.Add("@SessionId", SqlDbType.BigInt).Value = bill.SessionId;
            cmd.Parameters.Add("@Summ", SqlDbType.Int).Value = bill.Summ;
            cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = bill.CreateTime;

            DataSet sqlResult = db.GetDataSet(cmd);
            Bill result = ParseBill(sqlResult.Tables[0].Rows[0]);
            return result;
        }

        private Bill ParseBill(DataRow Row)
        {
            long id = (long)Row["Id"];
            long sessionId = (long)Row["SessionId"];
            int summ = (int)Row["Summ"];
            DateTime date = (DateTime)Row["Date"];
            Bill result = new Bill() { Id = id, SessionId = sessionId, Summ = summ, CreateTime = date };
            return result;
        }
    }
}
