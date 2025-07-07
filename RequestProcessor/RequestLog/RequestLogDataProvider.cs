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

namespace Hermes.Parking.Server.RequestProcessor
{
    public class RequestLogDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService db;
        private ISecurityManager sm;

        public RequestLogDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
            this.sm = Context.GetService<ISecurityManager>(Hermes.Parking.Server.RequestProcessor.Constants.SECURITY_MANAGER_NAME);
        }

        public override string GetName()
        {
            return Constants.REQUEST_LOG_DATA_PROVIDER_NAME;
        }

        public override void Create(string ObjectType, object InitialObject, ref object Output)
        {
            if (!(InitialObject is RequestLog))
                throw new InvalidCastException("Not 'RequestLog' object given");

            RequestLog log = (RequestLog)InitialObject;

            SqlCommand cmd = new SqlCommand(@"INSERT INTO RequestLog (RequestId, UserId, IsSuccess, TimeStamp, RequestData, ResponseData, ResponseMessage) OUTPUT inserted.* VALUES (@RequestId, @UserId, @IsSuccess, @TimeStamp, @RequestData, @ResponseData, @ResponseMessage);");
            cmd.Parameters.Add("@RequestId", SqlDbType.Int).Value = log.RequestId;
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = log.UserId;
            cmd.Parameters.Add("@IsSuccess", SqlDbType.Bit).Value = log.IsSuccess;
            cmd.Parameters.Add("@TimeStamp", SqlDbType.DateTime).Value = log.TimeStamp;
            cmd.Parameters.Add("@RequestData", SqlDbType.VarChar).Value = log.RequestData;
            cmd.Parameters.Add("@ResponseData", SqlDbType.VarChar).Value = log.ResponseData;
            cmd.Parameters.Add("@ResponseMessage", SqlDbType.VarChar).Value = log.ResponseMessage;

            DataSet sqlResult = db.GetDataSet(cmd);
            RequestLog result = ParseRequestLog(sqlResult.Tables[0].Rows[0]);
            Output = result;
        }

        public override void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
            string sql = @"SELECT * FROM RequestLog WHERE 1 = 1";

            if (Filter != null)
            {
                if (Filter.ContainsKey("TimeStampFrom"))
                    sql += string.Format(" AND TimeStamp >= '{0}'", ((DateTime)Filter["TimeStampFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));

                if (Filter.ContainsKey("TimeStampTo"))
                    sql += string.Format(" AND TimeStamp < '{0}'", ((DateTime)Filter["TimeStampTo"]).ToString("yyyy-MM-ddTHH:mm:ss"));

                if (Filter.ContainsKey("UserId"))
                    sql += string.Format(" AND UserId = {0}", (int)Filter["UserId"]);

                if (Filter.ContainsKey("RequestId"))
                    sql += string.Format(" AND UserId = {0}", (int)Filter["RequestId"]);

                if (Filter.ContainsKey("IsSuccess"))
                    sql += string.Format(" AND IsSuccess = {0}", (int)Filter["IsSuccess"]);
            }

            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);

            IList<RequestLog> result = new List<RequestLog>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                RequestLog tmp = ParseRequestLog(row);
                result.Add(tmp);
            }

            Output = (IEnumerable<object>)result;
        }

        private RequestLog ParseRequestLog(DataRow Row)
        {
            long id = (long)Row["Id"];
            int requestId = (int)Row["RequestId"];
            int userId = (int)Row["UserId"];
            DateTime timeStamp = (DateTime)Row["TimeStamp"];
            string requestData = (string)Row["RequestData"];
            bool isSuccess = (bool)Row["IsSuccess"];
            string responseData = (string)Row["ResponseData"];
            string responseMessage = (string)Row["ResponseMessage"];

            RequestLog result = new RequestLog()
            {
                Id = id,
                RequestId = requestId,
                UserId = userId,
                TimeStamp = timeStamp,
                RequestData = requestData,
                IsSuccess = isSuccess,
                ResponseData = responseData,
                ResponseMessage = responseMessage
            };

            return result;
        }
    }
}
