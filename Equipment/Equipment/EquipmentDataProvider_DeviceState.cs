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
        private DeviceState CreateDeviceState(DeviceState State)
        {
            SqlCommand cmd = new SqlCommand(@"INSERT INTO DeviceState (DeviceId, TimeStamp, State) OUTPUT inserted.* VALUES (@DeviceId, @TimeStamp, @State);");
            cmd.Parameters.Add("@DeviceId", SqlDbType.Int).Value = State.DeviceId;
            cmd.Parameters.Add("@TimeStamp", SqlDbType.DateTime).Value = State.TimeStamp;
            cmd.Parameters.Add("@State", SqlDbType.VarChar).Value = State.State;
            DataSet sqlResult = db.GetDataSet(cmd);
            DeviceState result = ParseDeviceState(sqlResult.Tables[0].Rows[0]);
            return result;
        }

        private IEnumerable<DeviceState> GetDevicesStates(IDictionary<string, object> Filter)
        {
            string sql = @"SELECT * FROM DeviceState WHERE 1 = 1";
            if (Filter != null)
            {
                if (Filter.ContainsKey("IsLast"))
                    sql = "SELECT s1.* FROM DeviceState AS s1 JOIN Device AS d ON d.Id = s1.DeviceId AND d.IsActive = 1 LEFT JOIN DeviceState AS s2 on s2.DeviceId = s1.DeviceId and s2.TimeStamp > s1.TimeStamp WHERE s2.Id IS NULL";
            }
            SqlCommand cmd = new SqlCommand(sql);
            DataSet sqlResult = db.GetDataSet(cmd);

            IList<DeviceState> result = new List<DeviceState>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                DeviceState state = ParseDeviceState(row);
                result.Add(state);
            }
            return result;
        }

        private DeviceState ParseDeviceState(DataRow Row)
        {
            int deviceId = (int)Row["DeviceId"];
            DateTime timeStamp = (DateTime)Row["TimeStamp"];
            string state = (string)Row["State"];

            DeviceState result = new DeviceState()
            {
                DeviceId = deviceId,
                TimeStamp = timeStamp,
                State = state
            };

            return result;
        }
    }
}
