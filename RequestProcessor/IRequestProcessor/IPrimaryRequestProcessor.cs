using System.Collections.Generic;

namespace Hermes.Parking.Server.RequestProcessor
{
    /// <summary>
    /// Сервис делегирует запросы последовательно всем подписанным на запрос обработчикам.
    /// </summary>
    public interface IPrimaryRequestProcessor
    {
        /// <summary>
        /// Список зарегистрированных запросов и подписанных на них обработчиков.
        /// </summary>
        IDictionary<string, IList<IRequestProcessor>> Requests { get; }

        /// <summary>
        /// Обработка входящего запроса последовательно всеми подписанными на запрос обработчиками.
        /// </summary>
        Response ProcessRequest(User User, Request Request);
    }
}
