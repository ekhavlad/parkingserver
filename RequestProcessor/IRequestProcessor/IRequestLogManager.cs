using System;
using System.Collections.Generic;

namespace Hermes.Parking.Server.RequestProcessor
{
    public interface IRequestLogManager
    {
        RequestLog AddRequestLog(RequestLog RequestLog);
        IEnumerable<RequestLog> GetRequestLogs(IDictionary<string, object> Filter);
    }
}
