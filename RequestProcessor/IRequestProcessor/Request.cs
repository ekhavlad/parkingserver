using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Parking.Server.RequestProcessor
{
    public struct Request
    {
        public string RequestName;

        public IDictionary<string, object> Data;

        public Request(string RequestName)
        {
            this.RequestName = RequestName;
            this.Data = null;
        }

        public Request(string RequestName, IDictionary<string, object> Data)
        {
            this.RequestName = RequestName;
            this.Data = Data;
        }
    }
}
