using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using Hermes.Parking.Server.EventService;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class RequestLogManager : BaseService, IRequestLogManager
    {
        private IDataService dataService;
        private IEventService eventService;

        public override string GetName() { return Constants.REQUEST_LOG_MANAGER_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            this.dataService = Context.GetService<IDataService>(Hermes.Parking.Server.DataService.Constants.DATA_SERVICE_NAME);
            this.eventService = Context.GetService<IEventService>(Hermes.Parking.Server.EventService.Constants.EVENT_SERVICE_NAME);
        }

        public RequestLog AddRequestLog(RequestLog RequestLog)
        {
            try
            {
                Logger.DebugFormat("Добавляем новый лог запроса RequestId:{0}/UserId:{1}", RequestLog.RequestId, RequestLog.UserId);
                RequestLog newLog = dataService.Create<RequestLog>(Constants.REQUEST_LOG_OBJECT_TYPE_NAME, RequestLog);
                Dictionary<string, object> eventData = new Dictionary<string,object>();
                eventData["RequestLog"] = newLog;
                eventService.EvokeEvent(Constants.REQUEST_LOG_ADDED_EVENT_NAME, eventData);
                return newLog;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Ошибка при создании лога запроса", ex);
                return null;
            }
        }

        public IEnumerable<RequestLog> GetRequestLogs(IDictionary<string, object> Filter)
        {
            return dataService.GetList<RequestLog>(Constants.REQUEST_LOG_OBJECT_TYPE_NAME, Filter);
        }
    }
}
