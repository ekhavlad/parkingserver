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
        private PlacesLog CreateLog(object InitialObject)
        {
            PlacesLog placesLog = (PlacesLog)InitialObject;

            string sql = @"INSERT INTO PlacesLog (ZoneId, PlacesCount, PlacesOccupied) OUTPUT inserted.* VALUES (@ZoneId, @PlacesCount, @PlacesOccupied);";

            SqlCommand cmd = new SqlCommand(sql);
            cmd.Parameters.Add("@ZoneId", SqlDbType.Int).Value = placesLog.ZoneId;
            cmd.Parameters.Add("@PlacesCount", SqlDbType.Int).Value = placesLog.PlacesCount;
            cmd.Parameters.Add("@PlacesOccupied", SqlDbType.Int).Value = placesLog.PlacesOccupied;

            DataSet sqlResult = db.GetDataSet(cmd);
            return ParsePlacesLog(sqlResult.Tables[0].Rows[0]);
        }

        private PlacesLog GetLastPlacesLogByZoneId(int ZoneId)
        {
            string sql = @"SELECT TOP 1 * FROM PlacesLog WHERE ZoneId = @ZoneId ORDER BY InsertStamp DESC";

            SqlCommand cmd = new SqlCommand(sql);
            cmd.Parameters.Add("@ZoneId", SqlDbType.Int).Value = ZoneId;
            DataSet sqlResult = db.GetDataSet(cmd);
            if (sqlResult.Tables[0].Rows.Count == 0)
                return null;
            else
                return ParsePlacesLog(sqlResult.Tables[0].Rows[0]);
        }

        private IEnumerable<PlacesLog> GetPlacesLogs(IDictionary<string, object> Filter)
        {
            string sql = "SELECT * FROM PlacesLog WHERE 1 = 1";

            if (Filter != null)
            {
                if (Filter.ContainsKey("ZoneId"))
                    sql += string.Format(" AND ZoneId = {0}", (int)Filter["ZoneId"]);

                if (Filter.ContainsKey("InsertStampFrom"))
                    sql += string.Format(" AND InsertStamp >= '{0}'", ((DateTime)Filter["InsertStampFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));

                if (Filter.ContainsKey("InsertStampTo"))
                    sql += string.Format(" AND InsertStamp < '{0}'", ((DateTime)Filter["InsertStampTo"]).ToString("yyyy-MM-ddTHH:mm:ss"));
            }

            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);
            List<PlacesLog> result = new List<PlacesLog>();
            foreach (DataRow r in sqlResult.Tables[0].Rows)
                result.Add(ParsePlacesLog(r));

            return (IEnumerable<PlacesLog>)result;
        }

        private PlacesLog ParsePlacesLog(DataRow Row)
        {
            long id = (long)Row["Id"];
            int zoneId = (int)Row["ZoneId"];
            int placesCount = (int)Row["PlacesCount"];
            int placesOccupied = (int)Row["PlacesOccupied"];
            DateTime insertStamp = (DateTime)Row["InsertStamp"];

            PlacesLog result = new PlacesLog()
            {
                Id = id,
                ZoneId = zoneId,
                PlacesCount = placesCount,
                PlacesOccupied = placesOccupied,
                InsertStamp = insertStamp
            };

            return result;
        }
    }
}
