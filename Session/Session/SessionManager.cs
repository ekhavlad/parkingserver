using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using Hermes.Parking.Server.EventService;
using Hermes.Parking.Server.FinanceService;
using Hermes.Parking.Server.Equipment;

namespace Hermes.Parking.Server.Session
{
    public class SessionManager : BaseService, ISessionManager
    {
        private IDataService dataService;
        private IEventService eventService;
        private IFinanceService financeService;
        private IEquipmentManager equipmentManager;
        

        public Dictionary<TicketType, ITicketNumberGenerator> NumberGenerators = new Dictionary<TicketType, ITicketNumberGenerator>();
        private object locker = new object();

        public override string GetName() { return Constants.SESSION_MANAGER_NAME; }

        public void AddTicketNumberGenerator(ITicketNumberGenerator Generator)
        {
            lock (locker)
            {
                NumberGenerators[Generator.TicketType] = Generator;
            }
        }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            this.dataService = Context.GetService<IDataService>(Hermes.Parking.Server.DataService.Constants.DATA_SERVICE_NAME);
            this.eventService = Context.GetService<IEventService>(Hermes.Parking.Server.EventService.Constants.EVENT_SERVICE_NAME);
            this.financeService = Context.GetService<IFinanceService>(Hermes.Parking.Server.FinanceService.Constants.FINANCE_SERVICE_NAME);
            this.equipmentManager = Context.GetService<IEquipmentManager>(Hermes.Parking.Server.Equipment.Constants.EQUIPMENT_MANAGER_NAME);
        }


        public Session CreateSession(TicketType TicketType, int RateId)
        {
            lock (locker)
            {
                if (!NumberGenerators.ContainsKey(TicketType))
                    throw new ServerDefinedException(string.Format("Нет генератора номеров типа {0}", TicketType));

                long ticketNumber = NumberGenerators[TicketType].GenerateNumber();

                return CreateSession(TicketType, ticketNumber, RateId);
            }
        }

        public Session CreateSession(TicketType TicketType, long TicketNumber, int RateId)
        {
            Rate rate = financeService.GetRateById(RateId);
            if (rate == null)
                throw new ServerDefinedException("Указан несуществующий тариф");

            Session session;
            DateTime GraceTime = DateTime.Now.AddMinutes(Constants.GRACE_PERIOD);

            lock (locker)
            {
                Session s = new Session()
                {
                    TicketType = TicketType,
                    TicketNumber = TicketNumber,
                    RateId = RateId,
                    GraceTime = GraceTime
                };
                session = dataService.Create<Session>(Constants.SESSION_OBJECT_TYPE_NAME, s);
            }

            Logger.InfoFormat("Session created: {0}", session);
            eventService.EvokeEvent(Constants.SESSION_CREATED_EVENT_NAME, GetEventData(session));

            return session;
        }


        public Session GetSessionById(long Id)
        {
            return dataService.Get<Session>(Constants.SESSION_OBJECT_TYPE_NAME, Id);
        }

        public Session GetCurrentSessionByTicketNumber(long TicketNumber)
        {
            Dictionary<string, object> filter = new Dictionary<string, object>();
            filter["CurrentSessionTicketNumber"] = TicketNumber;
            return dataService.Get<Session>(Constants.SESSION_OBJECT_TYPE_NAME, filter);
        }

        public IEnumerable<Session> GetSessions(IDictionary<string, object> Filter)
        {
            return dataService.GetList<Session>(Constants.SESSION_OBJECT_TYPE_NAME, Filter);
        }

        public void Save(Session Session)
        {
            Session original = GetSessionById(Session.Id);

            dataService.Save(Constants.SESSION_OBJECT_TYPE_NAME, Session);
            Logger.InfoFormat("Session changed: Id = {0}, CreateTime = {1:yyyy-MM-dd HH:mm:ss}, Status = {2}", Session.Id, Session.CreateTime, Session.Status);

            eventService.EvokeEvent(Constants.SESSION_CHANGED_EVENT_NAME, GetEventData(original, Session));
        }

        public void SetGraceTime(Session Session, DateTime GraceTime)
        {
            Session original = GetSessionById(Session.Id);
            Session.GraceTime = GraceTime;
            dataService.Save(Constants.SESSION_OBJECT_TYPE_NAME, Session);
            Logger.InfoFormat("Session changed: Id = {0}, CreateTime = {1:yyyy-MM-dd HH:mm:ss}, Status = {2}", Session.Id, Session.CreateTime, Session.Status);
            eventService.EvokeEvent(Constants.SESSION_CHANGED_EVENT_NAME, GetEventData(original, Session));
        }

        public void RefreshGraceTime(Session Session)
        {
            DateTime GraceTime = DateTime.Now.AddMinutes(Constants.GRACE_PERIOD);
            SetGraceTime(Session, GraceTime);
        }


        public int GetDebt(Session Session)
        {
            DateTime now = DateTime.Now;

            if (now < Session.GraceTime)
                return 0;

            Rate rate = financeService.GetRateById(Session.RateId);
            int debt = financeService.GetDebt(rate, Session.CreateTime, now);

            List<Payment> payments = financeService.GetSessionPayments(Session.Id).ToList();
            int totalPayments = payments.Select(x => x.Summ).Sum();
            if (totalPayments >= debt)
                return 0;

            if (payments.Exists(p => p.Date >= now.AddMinutes(-Constants.GRACE_PERIOD) && totalPayments >= financeService.GetDebt(rate, Session.CreateTime, p.Date)))
                return 0;

            return (totalPayments - debt);
        }

        public IEnumerable<Bill> GetSessionBills(long SessionId)
        {
            IDictionary<string, object> filter = new Dictionary<string, object>();
            filter["SessionId"] = SessionId;
            return dataService.GetList<Bill>(Constants.BILL_OBJECT_TYPE_NAME, filter);
        }

        public Bill CreateBill(Session Session)
        {
            Bill bill = new Bill()
            {
                SessionId = Session.Id,
                Summ = GetDebt(Session),
                CreateTime = DateTime.Now
            };

            bill = dataService.Create<Bill>(Constants.BILL_OBJECT_TYPE_NAME, bill);

            return null;
        }


        public bool IsDepartAvailable(Session Session)
        {
            DateTime now = DateTime.Now;

            if (now < Session.GraceTime)
                return true;

            Rate rate = financeService.GetRateById(Session.RateId);
            int debt = financeService.GetDebt(rate, Session.CreateTime, now);
            List<Payment> payments = financeService.GetSessionPayments(Session.Id).ToList();
            int totalPayments = payments.Select(x => x.Summ).Sum();

            return (totalPayments >= debt);
        }

        public void Close(Session Session)
        {
            Session.CancelTime = DateTime.Now;
            Session.Status = SessionStatus.ManuallyClosed;
            Save(Session);
        }

        public void Cancel(Session Session)
        {
            Session.CancelTime = DateTime.Now;
            Session.Status = SessionStatus.Cancelled;
            Save(Session);
        }

        public void Arrive(Session Session, int GateId)
        {
            Device gate = equipmentManager.GetDeviceById(GateId);
            if (gate == null)
                throw new ServerDefinedException("Указана несуществующая стойка");

            Session.ArriveTime = DateTime.Now;
            Session.Status = SessionStatus.VisitorArrived;
            Session.GateInId = GateId;
            Save(Session);
        }

        public void Depart(Session Session, int GateId)
        {
            Device gate = equipmentManager.GetDeviceById(GateId);
            if (gate == null)
                throw new ServerDefinedException("Указана несуществующая стойка");

            Session.DepartTime = DateTime.Now;
            Session.Status = SessionStatus.VisitorDeparted;
            Session.GateOutId = GateId;
            Save(Session);
        }


        private Dictionary<string, object> GetEventData(Session Session)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["Session"] = Session;
            return data;
        }

        private Dictionary<string, object> GetEventData(Session OriginalSession, Session Session)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["Session"] = Session;
            data["OriginalSession"] = OriginalSession;
            return data;
        }
    }
}
