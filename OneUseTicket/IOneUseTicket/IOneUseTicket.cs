using System;
using Hermes.Parking.Server.Session;

namespace Hermes.Parking.Server.OneUseTicket
{
    public interface IOneUseTicket
    {
        long Id { get; }
        long Number { get; }
        ISession Session { get; }
        DateTime CreateTime { get; }
    }
}
