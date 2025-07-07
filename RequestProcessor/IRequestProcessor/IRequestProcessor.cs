using System.Collections.Generic;

namespace Hermes.Parking.Server.RequestProcessor
{
    /// <summary>
    /// Обрабочик входящих запросов.
    /// </summary>
    public interface IRequestProcessor
    {
        /// <summary>
        /// Имя обработчика запросов.
        /// </summary>
        string GetName();

        /// <summary>
        /// Обработка входящего запроса.
        /// </summary>
        /// <returns>Возвращает true в случае, когда последующие подписанные на запрос обработчики могут продолжить обработку.
        /// Возвращает false, если обработку запроса следует прекратить и вернуть клиенту текущий результат.</returns>
        bool ProcessRequest(User User, Request Request, Response Response);
    }
}
