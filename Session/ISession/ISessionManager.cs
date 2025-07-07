using System;
using System.Collections.Generic;

namespace Hermes.Parking.Server.Session
{
    public interface ISessionManager
    {
        Session CreateSession(TicketType TicketType, int RateId);
        Session CreateSession(TicketType TicketType, long TicketNumber, int RateId);

        Session GetSessionById(long Id);
        Session GetCurrentSessionByTicketNumber(long TicketNumber);
        IEnumerable<Session> GetSessions(IDictionary<string, object> Filter);

        void Save(Session Session);
        void SetGraceTime(Session Session, DateTime GraceTime);
        void RefreshGraceTime(Session Session);

        int GetDebt(Session Session);
        Bill CreateBill(Session Session);
        IEnumerable<Bill> GetSessionBills(long SessionId);

        bool IsDepartAvailable(Session Session);
        void Cancel(Session Session);
        void Close(Session Session);
        void Arrive(Session Session, int GateId);
        void Depart(Session Session, int GateId);
    }
}
