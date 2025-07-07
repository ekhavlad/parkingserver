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

namespace Hermes.Parking.Server.Equipment
{
    public partial class EquipmentDataProvider : BaseDataProvider
    {
        private DeviceEvent CreateDeviceEvent(DeviceEvent State)
        {
            SqlCommand cmd = new SqlCommand(@"INSERT INTO DeviceEvent (DeviceId, TypeId, TimeStamp, Value) OUTPUT inserted.* VALUES (@DeviceId, @TypeId, @TimeStamp, @Value);");
            cmd.Parameters.Add("@DeviceId", SqlDbType.Int).Value = State.DeviceId;
            cmd.Parameters.Add("@TypeId", SqlDbType.Int).Value = State.TypeId;
            cmd.Parameters.Add("@TimeStamp", SqlDbType.DateTime).Value = State.TimeStamp;
            cmd.Parameters.Add("@Value", SqlDbType.VarChar).Value = State.Value;
            DataSet sqlResult = db.GetDataSet(cmd);
            DeviceEvent result = ParseDeviceEvent(sqlResult.Tables[0].Rows[0]);
            return result;
        }

        private IEnumerable<DeviceEvent> GetDevicesEvents(IDictionary<string, object> Filter)
        {
            string sql = @"SELECT * FROM DeviceEvent WHERE 1 = 1";
            if (Filter != null)
            {
                if (Filter.ContainsKey("TimeStampFrom"))
                    sql += string.Format(" AND TimeStamp >= '{0}'", ((DateTime)Filter["TimeStampFrom"]).ToString("yyyy-MM-ddTHH:mm:ss"));
            }
            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);

            IList<DeviceEvent> result = new List<DeviceEvent>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                DeviceEvent tmp = ParseDeviceEvent(row);
                result.Add(tmp);
            }
            return result;
        }

        private DeviceEvent ParseDeviceEvent(DataRow Row)
        {
            int deviceId = (int)Row["DeviceId"];
            int typeId = (int)Row["TypeId"];
            DateTime timeStamp = (DateTime)Row["TimeStamp"];
            string value = (string)Row["Value"];

            DeviceEvent result = new DeviceEvent()
            {
                DeviceId = deviceId,
                TypeId = typeId,
                TimeStamp = timeStamp,
                Value = value
            };

            return result;
        }
    }
}
