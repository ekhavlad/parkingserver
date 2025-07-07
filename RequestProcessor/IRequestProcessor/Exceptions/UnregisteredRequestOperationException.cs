using System;

namespace Hermes.Parking.Server.RequestProcessor
{
    /// <summary>
    /// Попытка запроса, на который не подписан ни одни обработчик.
    /// </summary>
    public class UnregisteredRequestOperationException : Exception
    {
        public UnregisteredRequestOperationException(string OperationName)
            : base(OperationName)
        {
        }
    }
}
