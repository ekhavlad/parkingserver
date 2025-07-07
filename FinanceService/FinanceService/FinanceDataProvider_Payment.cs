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

namespace Hermes.Parking.Server.FinanceService
{
    public partial class FinanceDataProvider : BaseDataProvider
    {
        private Payment CreatePayment(Payment Payment)
        {
            SqlCommand cmd = new SqlCommand(@"INSERT INTO Payment (UserId, SessionId, Summ, [Date]) OUTPUT inserted.* VALUES (@UserId, @SessionId, @Summ, @Date);");
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = Payment.UserId;
            cmd.Parameters.Add("@SessionId", SqlDbType.BigInt).Value = (Payment.SessionId == null) ? (object)DBNull.Value : Payment.SessionId;
            cmd.Parameters.Add("@Summ", SqlDbType.Int).Value = Payment.Summ;
            cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = Payment.Date;

            DataSet sqlResult = db.GetDataSet(cmd);
            Payment result = ParsePayment(sqlResult.Tables[0].Rows[0]);
            return result;
        }

        private Payment GetPaymentById(int Id)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Payment WHERE Id = @PaymentId");
            cmd.Parameters.Add("@PaymentId", SqlDbType.Int).Value = Id;

            DataSet sqlResult = db.GetDataSet(cmd);

            if (sqlResult.Tables[0].Rows.Count == 0)
                return null;

            Payment result = ParsePayment(sqlResult.Tables[0].Rows[0]);
            return result;
        }

        private IEnumerable<Payment> GetPayments(IDictionary<string, object> Filter)
        {
            string sql = @"SELECT * FROM Payment WHERE 1 = 1";

            if (Filter != null)
            {
                if (Filter.ContainsKey("SessionId"))
                    sql += string.Format(" AND [SessionId] = {0}", (long)Filter["SessionId"]);

                if (Filter.ContainsKey("DateFrom"))
                    sql += string.Format(" AND [Date] >= '{0}'", ((DateTime)Filter["DateFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));

                if (Filter.ContainsKey("DateTo"))
                    sql += string.Format(" AND [Date] < '{0}'", ((DateTime)Filter["DateTo"]).ToString("yyyy-MM-ddTHH:mm:ss"));
            }
            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);

            IList<Payment> result = new List<Payment>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                Payment payment = ParsePayment(row);
                result.Add(payment);
            }
            return result;
        }

        private Payment ParsePayment(DataRow Row)
        {
            long id = (long)Row["Id"];
            int userId = (int)Row["UserId"];
            long? sessionId = (Row["SessionId"] == DBNull.Value) ? null : (long?)Row["SessionId"]; ;
            int summ = (int)Row["Summ"];
            DateTime date = (DateTime)Row["Date"];
            Payment result = new Payment() { Id = id, UserId = userId, SessionId = sessionId, Summ = summ, Date = date };
            return result;
        }
    }
}
