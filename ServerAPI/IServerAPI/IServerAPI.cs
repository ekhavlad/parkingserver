using System;
using System.Collections.Generic;
using System.ServiceModel;
using Hermes.Parking.Server.RequestProcessor;

namespace Hermes.Parking.Server.ServerAPI
{
    /// <summary>
    /// API сервера парковки
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IServerAPICallback), SessionMode = SessionMode.Required)]
    public interface IServerAPI
    {
        /// <summary>
        /// Логин
        /// </summary>
        [OperationContract(IsInitiating = true, IsOneWay = false, IsTerminating = false)]
        Response Login(string Login, string Password);

        /// <summary>
        /// Логин
        /// </summary>
        [OperationContract(IsInitiating = false, IsOneWay = false, IsTerminating = true)]
        Response Logout();

        /// <summary>
        /// Обработка любого входящего запроса
        /// </summary>
        [OperationContract(IsInitiating = false, IsOneWay = false, IsTerminating = false)]
        Response ProcessRequest(Request Request);
    }
}