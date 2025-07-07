using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Core.Interfaces;

namespace Hermes.Parking.Server.FinanceService
{
    public class FinanceServiceRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private IFinanceService financeService;
        private Hermes.Parking.Server.SerializationService.ISerializationService serializationService;

        public FinanceServiceRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.financeService = Context.GetService<IFinanceService>(Constants.FINANCE_SERVICE_NAME);
            this.serializationService = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
        }

        public string GetName() { return Constants.FINANCE_SERVICE_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "CreateRate":
                    return CreateRate(Request.Data, Response.Data);
                case "GetAllRates":
                    return GetAllRates(Request.Data, Response.Data);
                case "UpdateRate":
                    return UpdateRate(Request.Data, Response.Data);
                case "DeleteRate":
                    return DeleteRate(Request.Data, Response.Data);

                case "CreatePayment":
                    return CreatePayment(Request.Data, Response.Data);
                case "GetPayments":
                    return GetPayments(Request.Data, Response.Data);

                default:
                    return true;
            }
        }

        private Rate ParseRate(string JSON)
        {
            try
            {
                Rate result = serializationService.Deserialize<Rate>(JSON);
                if (result == null)
                    throw new ServerDefinedException("Неправильный формат входных данных тарифа");
                return result;
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Не удалось распарсить тариф: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Не удалось распарсить тариф", ex);
                throw new ServerDefinedException("Неправильный формат входных данных тарифа");
            }
        }

        public bool CreateRate(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Rate")) throw new ServerDefinedException("Нет поля Rate");
            Rate rate = financeService.CreateRate(ParseRate(InputData["Rate"].ToString()));
            OutputData["Rate"] = serializationService.Serialize(rate);
            return true;
        }

        public bool GetAllRates(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<Rate> rates = financeService.GetAllRates();
            OutputData["Rates"] = serializationService.Serialize(rates);
            return true;
        }

        public bool UpdateRate(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Rate")) throw new ServerDefinedException("Нет поля Rate");
            Rate rate = ParseRate(InputData["Rate"].ToString());
            financeService.SaveRate(rate);
            OutputData["Rate"] = serializationService.Serialize(rate);
            return true;
        }

        public bool DeleteRate(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Rate")) throw new ServerDefinedException("Нет поля Rate");
            Rate rate = ParseRate(InputData["Rate"].ToString());
            financeService.DeleteRate(rate);
            return true;
        }





        private Payment ParsePayment(string JSON)
        {
            try
            {
                Payment result = serializationService.Deserialize<Payment>(JSON);
                if (result == null)
                    throw new ServerDefinedException("Неправильный формат входных данных платежа");
                return result;
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Не удалось распарсить платеж: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Не удалось распарсить платеж", ex);
                throw new ServerDefinedException("Неправильный формат входных данных платеж");
            }
        }

        public bool GetPayments(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<Payment> payments = financeService.GetPayments(InputData);
            OutputData["Payments"] = serializationService.Serialize(payments);
            return true;
        }

        public bool CreatePayment(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Payment")) throw new ServerDefinedException("Нет поля Payment");
            Payment payment = financeService.CreatePayment(ParsePayment(InputData["Payment"].ToString()));
            OutputData["Payment"] = serializationService.Serialize(payment);
            return true;
        }
    }
}
