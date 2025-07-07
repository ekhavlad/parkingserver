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
        private PassageLog CreateLog(object InitialObject)
        {
            PassageLog passageLog = (PassageLog)InitialObject;

            string sql = @"INSERT INTO PassageLog (GateId) OUTPUT inserted.* VALUES (@GateId);";

            SqlCommand cmd = new SqlCommand(sql);
            cmd.Parameters.Add("@GateId", SqlDbType.Int).Value = passageLog.GateId;

            DataSet sqlResult = db.GetDataSet(cmd);
            return ParsePassageLog(sqlResult.Tables[0].Rows[0]);
        }

        private IEnumerable<PassageLog> GetPassageLogs(IDictionary<string, object> Filter)
        {
            string sql = "SELECT * FROM PassageLog WHERE 1 = 1";

            if (Filter != null)
            {
                if (Filter.ContainsKey("GateId"))
                    sql += string.Format(" AND GateId = {0}", (int)Filter["GateId"]);

                if (Filter.ContainsKey("InsertStampFrom"))
                    sql += string.Format(" AND InsertStamp >= '{0}'", ((DateTime)Filter["InsertStampFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));

                if (Filter.ContainsKey("InsertStampTo"))
                    sql += string.Format(" AND InsertStamp < '{0}'", ((DateTime)Filter["InsertStampTo"]).ToString("yyyy-MM-ddTHH:mm:ss"));
            }

            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);
            List<PassageLog> result = new List<PassageLog>();
            foreach (DataRow r in sqlResult.Tables[0].Rows)
                result.Add(ParsePassageLog(r));

            return (IEnumerable<PassageLog>)result;
        }

        private PassageLog ParsePassageLog(DataRow Row)
        {
            long id = (long)Row["Id"];
            int gateId = (int)Row["GateId"];
            DateTime insertStamp = (DateTime)Row["InsertStamp"];

            PassageLog result = new PassageLog()
            {
                Id = id,
                GateId = gateId,
                InsertStamp = insertStamp
            };

            return result;
        }
    }
}
