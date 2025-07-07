using System.Collections.Generic;
using Hermes.Parking.Server.FinanceService;

namespace Hermes.Parking.Server.OneUseTicket
{
    public interface IOneUseTicketManager
    {
        IOneUseTicket CreateTicket(Session.ISession Session);
        IOneUseTicket GetTicketById(long Id);

        void Arrive(IOneUseTicket Ticket);
        void Leave(IOneUseTicket Ticket);
        bool IsDepartAvailable(IOneUseTicket Ticket, out string Reason);
        void Depart(IOneUseTicket Ticket);
    }
}
