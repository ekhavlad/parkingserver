using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Parking.Server.RequestProcessor
{
    public struct Response
    {
        public bool IsSuccess;

        public string Message;

        public IDictionary<string, object> Data;

        public Response(bool IsSuccess)
        {
            this.IsSuccess = IsSuccess;
            this.Message = "";
            this.Data = null;
        }

        public Response(bool IsSuccess, string Message)
        {
            this.IsSuccess = IsSuccess;
            this.Message = Message;
            this.Data = null;
        }

        public Response(bool IsSuccess, string Message, IDictionary<string, object> Data)
        {
            this.IsSuccess = IsSuccess;
            this.Message = Message;
            this.Data = Data;
        }
    }
}
