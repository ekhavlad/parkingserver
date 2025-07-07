using System;
using System.Collections.Generic;

namespace Hermes.Parking.Server.EventService
{
    public delegate void EventHandler(string EventName, IDictionary<string, object> Data);

    public interface IEventService
    {
        event EventHandler OnEvent;
        void EvokeEvent(string Message);
        void EvokeEvent(SystemEventLevel Level, string Message);
        void EvokeEvent(string EventName, string Message);
        void EvokeEvent(string EventName, IDictionary<string, object> Data);
    }
}
