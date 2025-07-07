using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Parking.Server.ServerAPI
{
    public struct CallbackCommand
    {
        public string CommandName;

        public IDictionary<string, object> Data;

        public CallbackCommand(string CommandName)
        {
            this.CommandName = CommandName;
            this.Data = null;
        }

        public CallbackCommand(string CommandName, IDictionary<string, object> Data)
        {
            this.CommandName = CommandName;
            this.Data = Data;
        }
    }
}
