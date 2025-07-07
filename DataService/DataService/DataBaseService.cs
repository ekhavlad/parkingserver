using System.Data.SqlClient;
using Hermes.Core.Interfaces;
using System.Data;

namespace Hermes.Parking.Server.DataService
{
    public class DataBaseService : BaseService, IDataBaseService
    {
        public override string GetName() { return Constants.DATABASE_SERVICE_NAME; }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(Configuration.ConnectionString);
        }

        public DataSet GetDataSet(SqlCommand Command)
        {
            using (SqlConnection conn = GetConnection())
            {
                using (Command)
                {
                    Command.Connection = conn;
                    using (SqlDataAdapter a = new SqlDataAdapter(Command))
                    {
                        DataSet result = new DataSet();
                        a.Fill(result);
                        return result;
                    }
                }
            }
        }

        public void Execute(SqlCommand Command)
        {
            GetDataSet(Command);
        }
    }
}