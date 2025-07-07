using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Hermes.Parking.Server.ServerAPI
{
    /// <summary>
    /// Callback API сервера парковки
    /// </summary>
    public interface IServerAPICallback
    {
        /// <summary>
        /// Обработка команды на клиенте
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ProcessCommand(CallbackCommand Command);

        /// <summary>
        /// Пинг клиента
        /// </summary>
        [OperationContract(IsOneWay = false)]
        void Ping();
    }
}
