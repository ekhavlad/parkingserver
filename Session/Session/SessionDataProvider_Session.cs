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
        private Session CreateSession(object InitialObject)
        {
            Session session = (Session)InitialObject;

            string sql = @" DECLARE @id bigint;
                            INSERT INTO Session (StatusId, TicketTypeId, TicketNumber, RateId, GraceTime) OUTPUT inserted.* VALUES (@StatusId, @TicketTypeId, @TicketNumber, @RateId, @GraceTime);
                            SELECT @id = SCOPE_IDENTITY();
                            INSERT INTO SessionStatusHistory(SessionId, StatusId) VALUES (@id, @StatusId);";

            SqlCommand cmd = new SqlCommand(sql);
            cmd.Parameters.Add("@StatusId", SqlDbType.Int).Value = SessionStatus.New;
            cmd.Parameters.Add("@TicketTypeId", SqlDbType.Int).Value = (int)session.TicketType;
            cmd.Parameters.Add("@TicketNumber", SqlDbType.BigInt).Value = session.TicketNumber;
            cmd.Parameters.Add("@RateId", SqlDbType.Int).Value = session.RateId;
            cmd.Parameters.Add("@GraceTime", SqlDbType.DateTime).Value = session.GraceTime;

            DataSet sqlResult = db.GetDataSet(cmd);
            return ParseSession(sqlResult.Tables[0].Rows[0]);
        }

        private IEnumerable<Session> GetSessions(IDictionary<string, object> Filter)
        {
            string sql = "SELECT * FROM Session WHERE 1 = 1";

            if (Filter != null)
            {
                if (Filter.ContainsKey("Id"))
                    sql += string.Format(" AND Id = {0}", (long)Filter["Id"]);

                if (Filter.ContainsKey("StatusId"))
                    sql += string.Format(" AND StatusId = {0}", (int)Filter["StatusId"]);

                if (Filter.ContainsKey("TicketTypeId"))
                    sql += string.Format(" AND TicketTypeId = {0}", (int)Filter["TicketTypeId"]);

                if (Filter.ContainsKey("TicketNumber"))
                    sql += string.Format(" AND TicketNumber = {0}", (long)Filter["TicketNumber"]);

                if (Filter.ContainsKey("ArriveTimeFrom"))
                    sql += string.Format(" AND ArriveTime >= '{0}'", ((DateTime)Filter["ArriveTimeFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                if (Filter.ContainsKey("ArriveTimeTo"))
                    sql += string.Format(" AND ArriveTime < '{0}'", ((DateTime)Filter["ArriveTimeTo"]).ToString("yyyy-MM-ddTHH:mm:ss"));

                if (Filter.ContainsKey("DepartTimeFrom"))
                    sql += string.Format(" AND DepartTime >= '{0}'", ((DateTime)Filter["DepartTimeFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                if (Filter.ContainsKey("DepartTimeTo"))
                    sql += string.Format(" AND DepartTime < '{0}'", ((DateTime)Filter["DepartTimeTo"]).ToString("yyyy-MM-ddTHH:mm:ss"));

                if (Filter.ContainsKey("CancelTimeFrom"))
                    sql += string.Format(" AND CancelTime >= '{0}'", ((DateTime)Filter["CancelTimeFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                if (Filter.ContainsKey("CancelTimeTo"))
                    sql += string.Format(" AND CancelTime < '{0}'", ((DateTime)Filter["CancelTimeTo"]).ToString("yyyy-MM-ddTHH:mm:ss"));

                if (Filter.ContainsKey("GateInId"))
                    sql += string.Format(" AND GateInId = {0}", (int)Filter["GateInId"]);

                if (Filter.ContainsKey("GateOutId"))
                    sql += string.Format(" AND GateOutId = {0}", (int)Filter["GateOutId"]);

                if (Filter.ContainsKey("CreateTimeFrom"))
                    sql += string.Format(" AND InsertStamp >= '{0}'", ((DateTime)Filter["CreateTimeFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                if (Filter.ContainsKey("CreateTimeTo"))
                    sql += string.Format(" AND InsertStamp < '{0}'", ((DateTime)Filter["CreateTimeTo"]).ToString("yyyy-MM-ddTHH:mm:ss"));

                if (Filter.ContainsKey("LastChangeTimeFrom"))
                    sql += string.Format(" AND EditStamp >= '{0}'", ((DateTime)Filter["LastChangeTimeFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));
                if (Filter.ContainsKey("LastChangeTimeTo"))
                    sql += string.Format(" AND EditStamp < '{0}'", ((DateTime)Filter["LastChangeTimeTo"]).ToString("yyyy-MM-ddTHH:mm:ss"));
            }

            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);
            List<Session> result = new List<Session>();
            foreach (DataRow r in sqlResult.Tables[0].Rows)
                result.Add(ParseSession(r));

            return (IEnumerable<Session>)result;
        }

        private Session ParseSession(DataRow Row)
        {
            long id = (long)Row["Id"];
            int statusId = (int)Row["StatusId"];
            int ticketTypeId = (int)Row["TicketTypeId"];
            long ticketNumber = (long)Row["TicketNumber"];
            int rateId = (int)Row["RateId"];

            DateTime? arriveTime = (Row["ArriveTime"] == DBNull.Value) ? (DateTime?)null : (DateTime?)Row["ArriveTime"];
            DateTime? departTime = (Row["DepartTime"] == DBNull.Value) ? (DateTime?)null : (DateTime?)Row["DepartTime"];
            DateTime? cancelTime = (Row["CancelTime"] == DBNull.Value) ? (DateTime?)null : (DateTime?)Row["CancelTime"];

            int? gateInId = (Row["GateInId"] == DBNull.Value) ? (int?)null : (int?)Row["GateInId"];
            int? gateOutId = (Row["GateOutId"] == DBNull.Value) ? (int?)null : (int?)Row["GateOutId"];

            DateTime graceTime = (DateTime)Row["GraceTime"];

            DateTime createTime = (DateTime)Row["InsertStamp"];
            DateTime editTime = (DateTime)Row["EditStamp"];

            Session result = new Session()
            {
                Id = id,
                Status = (SessionStatus)statusId,
                TicketType = (TicketType)ticketTypeId,
                TicketNumber = ticketNumber,

                RateId = rateId,

                ArriveTime = arriveTime,
                DepartTime = departTime,
                CancelTime = cancelTime,

                GateInId = gateInId,
                GateOutId = gateOutId,

                GraceTime = graceTime,

                CreateTime = createTime,
                LastChangeTime = editTime
            };

            return result;
        }
    }
}
