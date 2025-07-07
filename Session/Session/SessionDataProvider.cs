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
        private ILogger Logger;
        private IDataBaseService db;

        public SessionDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
        }

        public override string GetName()
        {
            return Constants.SESSION_DATA_PROVIDER_NAME;
        }


        public override void Create(string ObjectType, object InitialObject, ref object Output)
        {
            switch (ObjectType)
            {
                case Constants.SESSION_OBJECT_TYPE_NAME:
                    Output = CreateSession(InitialObject);
                    break;

                case Constants.BILL_OBJECT_TYPE_NAME:
                    Output = CreateBill(InitialObject);
                    break;
            }
        }

        public override void Get(string ObjectType, long Id, ref object Output)
        {
            string sql = "SELECT * FROM Session WHERE Id = @Id";
            SqlCommand cmd = new SqlCommand(sql);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
            DataSet sqlResult = db.GetDataSet(cmd);

            if (sqlResult.Tables[0].Rows.Count == 0)
                Output = null;
            else
                Output = ParseSession(sqlResult.Tables[0].Rows[0]);
        }

        public override void Get(string ObjectType, IDictionary<string, object> Filter, ref object Output)
        {
            string sql = "SELECT * FROM Session WHERE 1 = 1";

            if (Filter.ContainsKey("CurrentSessionTicketNumber"))
                sql += string.Format(" AND TicketNumber = {0} AND StatusId IN ({1})",
                    (long)Filter["CurrentSessionTicketNumber"],
                    string.Join(",", Enum.GetValues(typeof(ActiveSessionStatus)).Cast<int>().ToArray()) // смотрим только активные сессии
                    );

            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);

            if (sqlResult.Tables[0].Rows.Count == 0)
                Output = null;
            else
                Output = ParseSession(sqlResult.Tables[0].Rows[0]);
        }

        public override void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
            switch (ObjectType)
            {
                case Constants.SESSION_OBJECT_TYPE_NAME:
                    Output = GetSessions(Filter);
                    break;

                case Constants.BILL_OBJECT_TYPE_NAME:
                    Output = GetBills(Filter);
                    break;
            }
        }

        public override void Save(string ObjectType, object Object)
        {
            Session s = (Session)Object;

            string sql = @"IF EXISTS(SELECT '' FROM [Session] WHERE Id = @Id AND StatusId <> @StatusId)
                                INSERT INTO SessionStatusHistory(SessionId, StatusId) VALUES (@Id, @StatusId);
                           UPDATE Session SET
                                StatusId = @StatusId,

                                TicketTypeId = @TicketTypeId,
                                TicketNumber = @TicketNumber,

                                RateId = @RateId,

                                ArriveTime = @ArriveTime,
                                DepartTime = @DepartTime,
                                CancelTime = @CancelTime,

                                GateInId = @GateInId,
                                GateOutId = @GateOutId,

                                GraceTime = @GraceTime,

                                EditStamp = GETDATE()
                                WHERE Id = @Id";

            SqlCommand cmd = new SqlCommand(sql);
            cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = s.Id;
            cmd.Parameters.Add("@StatusId", SqlDbType.Int).Value = s.Status;
            cmd.Parameters.Add("@TicketTypeId", SqlDbType.Int).Value = (int)s.TicketType;
            cmd.Parameters.Add("@TicketNumber", SqlDbType.BigInt).Value = s.TicketNumber;

            cmd.Parameters.Add("@RateId", SqlDbType.Int).Value = s.RateId;

            cmd.Parameters.Add("@ArriveTime", SqlDbType.DateTime).Value = (s.ArriveTime.HasValue) ? s.ArriveTime : (object)DBNull.Value;
            cmd.Parameters.Add("@DepartTime", SqlDbType.DateTime).Value = (s.DepartTime.HasValue) ? s.DepartTime : (object)DBNull.Value;
            cmd.Parameters.Add("@CancelTime", SqlDbType.DateTime).Value = (s.CancelTime.HasValue) ? s.CancelTime : (object)DBNull.Value;

            cmd.Parameters.Add("@GateInId", SqlDbType.Int).Value = (s.GateInId.HasValue) ? s.GateInId : (object)DBNull.Value;
            cmd.Parameters.Add("@GateOutId", SqlDbType.Int).Value = (s.GateOutId.HasValue) ? s.GateOutId : (object)DBNull.Value;

            cmd.Parameters.Add("@GraceTime", SqlDbType.DateTime).Value = s.GraceTime;

            db.Execute(cmd);
        }
    }
}
