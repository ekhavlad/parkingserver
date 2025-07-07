using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.SerializationService;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class PrimaryRequestProcessor : BaseService, IPrimaryRequestProcessor
    {
        private Dictionary<string, IList<IRequestProcessor>> requests = new Dictionary<string, IList<IRequestProcessor>>();
        public IDictionary<string, IList<IRequestProcessor>> Requests { get { return requests; } }

        private ISecurityManager securityManager;
        private IRequestLogManager requestLogManager;
        private ISerializationService serializationService;

        public override string GetName() { return Constants.PRIMARY_REQUEST_PROCESSOR_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            this.securityManager = Context.GetService<ISecurityManager>(Hermes.Parking.Server.RequestProcessor.Constants.SECURITY_MANAGER_NAME);
            this.requestLogManager = Context.GetService<IRequestLogManager>(Hermes.Parking.Server.RequestProcessor.Constants.REQUEST_LOG_MANAGER_NAME);
            this.serializationService = Context.GetService<ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
        }

        /// <summary>
        /// Подписывает обработчик на соответствующий запрос.
        /// </summary>
        /// <param name="RequestName">Имя запроса.</param>
        /// <param name="Processor">Обработчик.</param>
        public void AddRequestProcessor(string RequestName, IRequestProcessor Processor)
        {
            Logger.DebugFormat("Запрос {0}, обработчик {1}", RequestName, Processor.GetName());

            if (!requests.ContainsKey(RequestName))
            {
                requests.Add(RequestName, new List<IRequestProcessor>());
                Logger.InfoFormat("Запрос {0} зарегистрирован", RequestName);
            }
            
            if (!requests[RequestName].Contains(Processor))
            {
                requests[RequestName].Add(Processor);
                Logger.InfoFormat("Обработчик {0} подписан на запрос {1}", Processor.GetName(), RequestName);
            }
            // если этот обработчик уже подписан, то убираем его - он встанет в конец очереди
            else
            {
                requests[RequestName].Remove(Processor);
                requests[RequestName].Add(Processor);
                Logger.InfoFormat("Обработчик {0} переподписан на запрос {1}", Processor.GetName(), RequestName);
            }
        }

        /// <summary>
        /// Отписывает обработчик от соответствующего запроса.
        /// </summary>
        /// <param name="RequestName">Имя запроса.</param>
        /// <param name="Processor">Обработчик.</param>
        public void RemoveRequestProcessor(string RequestName, IRequestProcessor Processor)
        {
            Logger.DebugFormat("Запрос {0}, обработчик {1}", RequestName, Processor.GetName());

            if (!requests.ContainsKey(RequestName))
                Logger.DebugFormat("Запрос {0} не зарегистрирован", RequestName);

            else if (!requests[RequestName].Contains(Processor))
                Logger.DebugFormat("Обработчик {0} не подписан на запрос {1}", Processor.GetName(), RequestName);

            else
            {
                requests[RequestName].Remove(Processor);
                Logger.InfoFormat("Обработчик {0} отписан от запроса {1}", Processor.GetName(), RequestName);

                if (requests[RequestName].Count == 0)
                {
                    requests.Remove(RequestName);
                    Logger.InfoFormat("Запрос {0} больше не обслуживается ни одним обработчиком", RequestName);
                }
            }
        }

        public Response ProcessRequest(User User, Request Request)
        {
            Response response = new Response(true, "", new Dictionary<string, object>());

            try
            {
                // если пользователь не указан, считаем, что он не залоггирован
                if (User == null) throw new ServerDefinedException("Пользователь не вошел в систему");

                Logger.DebugFormat("Пользователь {0} ({1}), запрос {2}, входные параметры [{3}]",
                    User.Id,
                    User.Login,
                    Request.RequestName,
                    (Request.Data == null) ? "" : string.Format("{0}", string.Join(", ", Request.Data.Select(t => string.Format("{0}={1}", t.Key, t.Value)).ToArray())));

                // если запрос не зарегистрирован в сервисе
                if (!requests.ContainsKey(Request.RequestName)) throw new ServerDefinedException(string.Format("Запрос '{0}' не зарегистрирован в системе", Request.RequestName));
                // если у пользователя нет прав на обработку запроса
                if (!securityManager.IsRequestAvailable(User.Id, Request.RequestName)) throw new ServerDefinedException(string.Format("У вас нет прав на выполнение запроса '{0}'", Request.RequestName));

                for (int i = 0; i < requests[Request.RequestName].Count && requests[Request.RequestName][i].ProcessRequest(User, Request, response); i++) ;
            }
            catch (ServerDefinedException sEx)
            {
                Logger.Warn("Устранимая ошибка приложения", sEx);
                response.IsSuccess = false;
                response.Message = sEx.Message;
            }
            catch (Exception ex666)
            {
                Logger.ErrorFormat("Непредвиденная ошибка при обработке запроса {0}", ex666, Request.RequestName);
                response.IsSuccess = false;
                response.Message = "Ошибка сервера";
            }

            Logger.DebugFormat("Запрос {0}, ResultCode = {1}, Message = '{2}'",
                Request.RequestName,
                response.IsSuccess,
                response.Message);

            //Logger.DebugFormat("Запрос {0}, ResultCode = {1}, Message = '{2}',  OutputData: [{3}]",
            //    Request.RequestName,
            //    response.IsSuccess,
            //    response.Message,
            //    string.Format("{0}", string.Join(", ", response.Data.Select(t => string.Format("{0}={1}", t.Key, t.Value)).ToArray())));

            if (Request.RequestName != "GetRequestLogs")
            try
            {
                RequestLog log = new RequestLog()
                {
                    UserId = User.Id,
                    RequestId = securityManager.GetRequestByName(Request.RequestName).Id,
                    IsSuccess = response.IsSuccess,
                    TimeStamp = DateTime.Now,
                    ResponseData = serializationService.Serialize(response.Data),
                    ResponseMessage = response.Message,
                    RequestData = serializationService.Serialize(Request.Data)
                };
                requestLogManager.AddRequestLog(log);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка при добавлении лога запроса", ex);
            }

            return response;
        }
    }
}
