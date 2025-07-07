using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Core.Interfaces;

namespace Hermes.Parking.Server.EventService
{
    public class EventService : BaseService, IEventService
    {
        public override string GetName() { return Constants.EVENT_SERVICE_NAME; }

        public event EventHandler OnEvent;

        public void EvokeEvent(string EventName, IDictionary<string, object> Data)
        {
            OnEvent(EventName, Data);
        }

        public void EvokeEvent(string EventName, string Message)
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["Message"] = Message;
            EvokeEvent(EventName, Data);
        }

        public void EvokeEvent(SystemEventLevel Level, string Message)
        {
            SystemEvent ev = new SystemEvent() { Level = Level, Message = Message };
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["SystemEvent"] = ev;
            EvokeEvent(Constants.SYSTEM_EVENT_NAME, Data);
        }

        public void EvokeEvent(string Message)
        {
            EvokeEvent(SystemEventLevel.Info, Message);
        }
    }
}
