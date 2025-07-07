using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.SerializationService;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class RequestLogRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private IRequestLogManager requestLogManager;
        private ISerializationService serializationService;

        public RequestLogRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.requestLogManager = Context.GetService<IRequestLogManager>(Constants.REQUEST_LOG_MANAGER_NAME);
            this.serializationService = Context.GetService<ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
        }

        public string GetName() { return Constants.REQUEST_LOG_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Hermes.Parking.Server.RequestProcessor.Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "GetRequestLogs":
                    return GetRequestLogs(Request.Data, Response.Data);

                default:
                    return true;
            }
        }

        public bool GetRequestLogs(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<RequestLog> logs = requestLogManager.GetRequestLogs(InputData);
            OutputData["RequestLogs"] = serializationService.Serialize(logs);
            return true;
        }
    }
}
