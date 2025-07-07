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
        private Rate CreateRate(Rate Rate)
        {
            SqlCommand cmd = new SqlCommand(@"INSERT INTO Rate (TypeId, Name, Description, Config) OUTPUT inserted.* VALUES (@TypeId, @Name, @Description, @Config);");
            cmd.Parameters.Add("@TypeId", SqlDbType.Int).Value = (int)Rate.Type;
            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = Rate.Name;
            cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = Rate.Description;
            cmd.Parameters.Add("@Config", SqlDbType.VarChar).Value = (Rate.Config == null) ? (object)DBNull.Value : Rate.Config;
            DataSet sqlResult = db.GetDataSet(cmd);
            Rate result = ParseRate(sqlResult.Tables[0].Rows[0]);
            return result;
        }

        private Rate GetRateById(int Id)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Rate WHERE Id = @RateId");
            cmd.Parameters.Add("@RateId", SqlDbType.Int).Value = Id;

            DataSet sqlResult = db.GetDataSet(cmd);

            if (sqlResult.Tables[0].Rows.Count == 0)
                return null;

            Rate result = ParseRate(sqlResult.Tables[0].Rows[0]);
            return result;
        }

        private IEnumerable<Rate> GetRates(IDictionary<string, object> Filter)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Rate");
            DataSet sqlResult = db.GetDataSet(cmd);

            IList<Rate> result = new List<Rate>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                Rate rate = ParseRate(row);
                result.Add(rate);
            }
            return result;
        }        

        private void SaveRate(Rate Rate)
        {
            SqlCommand cmd = new SqlCommand(@"
                UPDATE Rate SET
                    TypeId = @TypeId,
                    Name = @Name,
                    Description = @Description,
                    Config = @Config
                WHERE Id = @RateId");
            cmd.Parameters.Add("@RateId", SqlDbType.Int).Value = Rate.Id;
            cmd.Parameters.Add("@TypeId", SqlDbType.Int).Value = (int)Rate.Type;
            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = Rate.Name;
            cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = (Rate.Description == null) ? (object)DBNull.Value : Rate.Description;
            cmd.Parameters.Add("@Config", SqlDbType.VarChar).Value = (Rate.Config == null) ? (object)DBNull.Value : Rate.Config;

            db.Execute(cmd);
        }

        private void DeleteRate(Rate Rate)
        {
            SqlCommand cmd = new SqlCommand(@"DELETE FROM Gate WHERE Id = @Id");
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Rate.Id;
            db.Execute(cmd);
        }

        private Rate ParseRate(DataRow Row)
        {
            int id = (int)Row["Id"];
            int typeId = (int)Row["TypeId"];
            string name = (string)Row["Name"];
            string description = (Row["Description"] == DBNull.Value) ? null : (string)Row["Description"];
            string config = (Row["Config"] == DBNull.Value) ? null : (string)Row["Config"];

            Rate result = new Rate() { Id = id, Type = (RateType)typeId, Name = name, Description = description, Config = config };

            return result;
        }
    }
}
