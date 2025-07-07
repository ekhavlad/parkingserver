using System.Data.SqlClient;
using System.Data;

namespace Hermes.Parking.Server.DataService
{
    /// <summary>
    /// Задача этого сервиса - по запросу выдать готовый SqlConnection.
    /// </summary>
    public interface IDataBaseService
    {
        /// <summary>
        /// Возвращает SqlConnection для работы с БД.
        /// </summary>
        SqlConnection GetConnection();

        /// <summary>
        /// Возвращает DataSet на основе запроса Command.
        /// </summary>
        DataSet GetDataSet(SqlCommand Command);

        /// <summary>
        /// Выполняет запрос в БД без возвращаемого результата.
        /// </summary>
        void Execute(SqlCommand Command);
    }
}
