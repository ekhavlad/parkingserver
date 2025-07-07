using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.FinanceService;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;

namespace Hermes.Parking.Server.OneUseTicket
{
    public class OneUseTicketFinanceProvider : IFinanceProvider
    {
        private IContext context;
        private ILogger Logger;
        private IOneUseTicketManager tm;
        private IFinanceService fs;
        private IDataService ds;

        public OneUseTicketFinanceProvider(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.tm = Context.GetService<IOneUseTicketManager>(Constants.DEFAULT_ONE_USE_TICKET_MANAGER_NAME);
            this.fs = Context.GetService<IFinanceService>(FinanceService.Constants.DEFAULT_FINANCE_SERVICE_NAME);
            this.ds = Context.GetService<IDataService>(DataService.Constants.DEFAULT_DATA_SERVICE_NAME);
        }

        public string GetName() { return Constants.DEFAULT_ONE_USE_TICKET_FINANCE_PROVIDER_NAME; }

        private decimal CalculateToBePaid(IOneUseTicket Ticket)
        {
            // если статус сессии не позволяет расчитать баланс, то приравниваем его к 0
            if (Ticket.Session.Status != Session.SessionStatus.VisitorArrived)
            {
                Logger.InfoFormat("TicketId = {0}, SessionId = {1}, SessionStatus = {2} => Balance = 0.", Ticket.Id, Ticket.Session.Id, Ticket.Session.Status);
                return 0;
            }

            IEnumerable<IBill> bills = tm.GetBills(Ticket);

            // общая сумма, которую заплатил посетитель
            decimal totalPaid = bills.Where(x => x.ActualSumm.HasValue).Select(x => x.ActualSumm.Value).Sum();
            // сколько надо заплатить на текущий момент
            decimal toBePaid = (int)(DateTime.Now.AddSeconds(-15) - Ticket.Session.ArriveTime.Value).TotalSeconds * 5;

            // если уже больше заплатил, чем надо
            if (totalPaid >= toBePaid)
            {
                Logger.InfoFormat("TicketId = {0}, ToBePaid = {1}, totalPaid = {2} => Balance = 0.", Ticket.Id, toBePaid, totalPaid);
                return 0;
            }

            // если существует оплаченный счет, выставленный не ранее 15 секунд назад
            var tmp = bills.Where(x =>
                x.CreateTime >= DateTime.Now.AddSeconds(-15) &&
                x.ActualSumm.HasValue &&
                x.ActualSumm.Value >= x.MinSumm
                ).Select(x => x);

            if (tmp.Count() > 0)
            {
                IBill bill = tmp.First();
                Logger.InfoFormat("TicketId = {0}, BillId = {1}, MinSum = {2} CreateDate = {3:yyyy-MM-dd HH:mm:ss}, ActualAmount = {4}, PaymentDate = {5:yyyy-MM-dd HH:mm:ss} => Balance = 0.", Ticket.Id, bill.Id, bill.CreateTime, bill.MinSumm, bill.ActualSumm, bill.PaymentTime.Value);
                return 0;
            }

            Logger.InfoFormat("TicketId = {0}, ToBePaid = {1}, TotalPaid = {2}", Ticket.Id, toBePaid, totalPaid);

            return toBePaid - totalPaid;
        }

        public bool GetBalance(string OperationType, IDictionary<string, object> Data, ref decimal Balance, IDictionary<string, object> AdditionalInfo)
        {
            IOneUseTicket ticket = ParseTicket(Data);
            Balance = -CalculateToBePaid(ticket);
            return true;
        }

        public bool CreateBill(string OperationType, IDictionary<string, object> Data, IBill Bill, IDictionary<string, object> AdditionalInfo)
        {
            IOneUseTicket ticket = ParseTicket(Data);
            decimal toBePaid = -fs.GetBalance("OneUseTicket", Data, out AdditionalInfo);
            Bill.MinSumm = toBePaid;

            IDictionary<string, object> data = new Dictionary<string, object>();
            data = new Dictionary<string, object>();
            data["OneUseTicket"] = ticket;
            data["Bill"] = Bill;
            ds.CreateObject<IBill>(Constants.DEFAULT_ONE_USE_TICKET_BILL_DATA_PROVIDER_NAME, data);

            return true;
        }

        public bool Pay(string OperationType, IBill Bill, decimal Summ, IDictionary<string, object> Data, IDictionary<string, object> AdditionalInfo)
        {
            return true;
        }


        private IOneUseTicket ParseTicket(IDictionary<string, object> Data)
        {
            return (IOneUseTicket)Data["OneUseTicket"];
        }
    }
}
