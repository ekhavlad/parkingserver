using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class RequestLog
    {
        public long Id;
        public int RequestId;
        public int UserId;
        public DateTime TimeStamp;
        public string RequestData;
        public bool IsSuccess;
        public string ResponseData;
        public string ResponseMessage;
    }
}
