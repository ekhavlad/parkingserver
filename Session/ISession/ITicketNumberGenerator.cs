using System;
using System.Collections.Generic;

namespace Hermes.Parking.Server.Session
{
    public interface ITicketNumberGenerator
    {
        TicketType TicketType { get; }
        long GenerateNumber();
    }
}
