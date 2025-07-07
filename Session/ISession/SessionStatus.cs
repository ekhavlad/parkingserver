namespace Hermes.Parking.Server.Session
{
    public enum SessionStatus
    {
        New = 1,
        Cancelled = 2,
        ClientGotTicket = 3,
        VisitorArrived = 4,
        VisitorDeparted = 5,
        ManuallyClosed = 6
    }

    public enum ActiveSessionStatus
    {
        New = SessionStatus.New,
        ClientGotTicket = SessionStatus.ClientGotTicket,
        VisitorArrived = SessionStatus.VisitorArrived
    }
}
