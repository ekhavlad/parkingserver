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
        private Device CreateDevice(Device Device)
        {
            SqlCommand cmd = new SqlCommand(@"INSERT INTO Device (ParentId, TypeId, Name, Config) OUTPUT inserted.* VALUES (@ParentId, @TypeId, @Name, @Config);");
            cmd.Parameters.Add("@ParentId", SqlDbType.Int).Value = (Device.ParentId.HasValue) ? Device.ParentId : (object)DBNull.Value;
            cmd.Parameters.Add("@TypeId", SqlDbType.Int).Value = (int)Device.Type;
            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = Device.Name;
            cmd.Parameters.Add("@Config", SqlDbType.VarChar).Value = (Device.Config == null) ?  (object)DBNull.Value: Device.Config;
            DataSet sqlResult = db.GetDataSet(cmd);
            Device result = ParseDevice(sqlResult.Tables[0].Rows[0]);
            return result;
        }

        private IEnumerable<Device> GetDevices(IDictionary<string, object> Filter)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Device WHERE IsActive = 1");
            DataSet sqlResult = db.GetDataSet(cmd);

            IList<Device> result = new List<Device>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                Device device = ParseDevice(row);
                result.Add(device);
            }
            return result;
        }

        private void SaveDevice(Device Device)
        {
            SqlCommand cmd = new SqlCommand(@"
                UPDATE Device SET
                    ParentId = @ParentId,
                    TypeId = @TypeId,
                    Name = @name,
                    Config = @Config
                WHERE Id = @Id");
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Device.Id;
            cmd.Parameters.Add("@ParentId", SqlDbType.Int).Value = (Device.ParentId.HasValue) ? Device.ParentId : (object)DBNull.Value;
            cmd.Parameters.Add("@TypeId", SqlDbType.Int).Value = (int)Device.Type;
            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = Device.Name;
            cmd.Parameters.Add("@Config", SqlDbType.VarChar).Value = (Device.Config == null) ? (object)DBNull.Value : Device.Config;
            db.Execute(cmd);
        }

        private void DeleteDevice(int DeviceId)
        {
            SqlCommand cmd = new SqlCommand(@"UPDATE Device SET IsActive = 0 WHERE Id = @Id");
            //SqlCommand cmd = new SqlCommand(@"DELETE FROM Device WHERE Id = @Id");
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = DeviceId;
            db.Execute(cmd);
       }

        private Device ParseDevice(DataRow Row)
        {
            int id = (int)Row["Id"];
            int? parentId = (Row["ParentId"] == DBNull.Value) ? null : (int?)Row["ParentId"];
            int typeId = (int)Row["TypeId"];
            string name = (string)Row["Name"];
            string config = (Row["Config"] == DBNull.Value) ? null : (string)Row["Config"];

            Device result = new Device()
            {
                Id = id,
                ParentId = parentId,
                Type = (DeviceType)typeId,
                Name = name,
                Config = config
            };

            return result;
        }
    }
}
