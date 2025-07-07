using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using Hermes.Parking.Server.FinanceService;
using Hermes.Parking.Server.Session;

namespace Hermes.Parking.Server.OneUseTicket
{
    public class OneUseTicketManager : BaseService, IOneUseTicketManager
    {
        private IDataService dataService;
        private IFinanceService fs;
        private ISessionManager sm;

        public override string GetName() { return Constants.DEFAULT_ONE_USE_TICKET_MANAGER_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            this.dataService = Context.GetService<IDataService>(DataService.Constants.DEFAULT_DATA_SERVICE_NAME);
            this.fs = Context.GetService<IFinanceService>(FinanceService.Constants.DEFAULT_FINANCE_SERVICE_NAME);
            this.sm = Context.GetService<ISessionManager>(Session.Constants.DEFAULT_SESSION_MANAGER_NAME);
        }


        public IOneUseTicket CreateTicket(Session.ISession Session)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["Session"] = Session;
            IOneUseTicket t = dataService.CreateObject<IOneUseTicket>(Constants.DEFAULT_ONE_USE_TICKET_DATA_PROVIDER_NAME, data);
            Logger.InfoFormat("OneUseTicket created: Id = {0}, CreateTime = {1:yyyy-MM-dd HH:mm:ss}, SessionId = {2}", t.Id, t.CreateTime, t.Session.Id);
            return t;
        }

        public IOneUseTicket GetTicketById(long Id)
        {
            return dataService.GetObject<IOneUseTicket>(Constants.DEFAULT_ONE_USE_TICKET_DATA_PROVIDER_NAME, Id);
        }

        
        public void Arrive(IOneUseTicket Ticket)
        {
            Ticket.Session.Status = Session.SessionStatus.VisitorArrived;
            Ticket.Session.ArriveTime = DateTime.Now;
            sm.Save(Ticket.Session);
        }

        public void Leave(IOneUseTicket Ticket)
        {
            Ticket.Session.Status = Session.SessionStatus.Cancelled;
            sm.Save(Ticket.Session);
        }

        public bool IsDepartAvailable(IOneUseTicket Ticket, out string Reason)
        {
            Reason = "";

            decimal balance = GetBalance(Ticket);

            if (balance < 0)
            {
                Reason = string.Format("Необходимо оплатить парковку на сумму {0} рублей", (-balance).ToString("N0"));
                return false;
            }

            switch (Ticket.Session.Status)
            {
                case Session.SessionStatus.New:
                    Reason = "Билет не активирован";
                    return false;

                case Session.SessionStatus.Cancelled:
                case Session.SessionStatus.ManuallyClosed:                
                case Session.SessionStatus.VisitorDeparted:
                    Reason = "Билет недействителен";
                    return false;

                default:
                    return true;
            }
        }

        public void Depart(IOneUseTicket Ticket)
        {
            Ticket.Session.Status = Session.SessionStatus.VisitorDeparted;
            Ticket.Session.DepartTime = DateTime.Now;
            sm.Save(Ticket.Session);
        }


        public decimal GetBalance(IOneUseTicket Ticket)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            data["OneUseTicket"] = Ticket;
            IDictionary<string, object> info = new Dictionary<string, object>();
            return fs.GetBalance("OneUseTicket", data, out info);
        }

        public IBill CreateBill(IOneUseTicket Ticket)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            data["OneUseTicket"] = Ticket;
            IDictionary<string, object> info = new Dictionary<string, object>();
            IBill bill = fs.CreateBill("OneUseTicket", data, out info);
            return bill;
        }

        public void Pay(IOneUseTicket Ticket, IBill Bill, decimal Summ)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            IDictionary<string, object> info = new Dictionary<string, object>();
            fs.Pay("OneUseTicket", Bill, Summ, data, out info);
        }


        public IEnumerable<IBill> GetBills(IOneUseTicket Ticket)
        {
            IDictionary<string, object> filter = new Dictionary<string, object>();
            filter.Add("OneUseTicket", Ticket);
            return dataService.GetObjects<IBill>(Constants.DEFAULT_ONE_USE_TICKET_BILL_DATA_PROVIDER_NAME, filter);
        }
    }
}
