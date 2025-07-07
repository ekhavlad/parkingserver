using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using Hermes.Parking.Server.EventService;

namespace Hermes.Parking.Server.FinanceService
{
    public class FinanceService : BaseService, IFinanceService
    {
        private IDataService dataService;
        private IEventService eventService;


        public override string GetName() { return Constants.FINANCE_SERVICE_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            this.dataService = Context.GetService<IDataService>(DataService.Constants.DATA_SERVICE_NAME);
            this.eventService = Context.GetService<IEventService>(Hermes.Parking.Server.EventService.Constants.EVENT_SERVICE_NAME);
        }

        private IDictionary<RateType, IDebtCalculator> calculators = new Dictionary<RateType, IDebtCalculator>();
        public IDictionary<RateType, IDebtCalculator> DebtCalculators { get { return calculators; } }
        public void AddDebtCalculator(RateType Type, IDebtCalculator Calculator)
        {
            calculators[Type] = Calculator;
        }


        public Rate GetRateById(int Id)
        {
            return dataService.Get<Rate>(Constants.RATE_OBJECT_TYPE_NAME, Id);
        }

        public IEnumerable<Rate> GetAllRates()
        {
            return dataService.GetList<Rate>(Constants.RATE_OBJECT_TYPE_NAME, null);
        }

        public Rate CreateRate(Rate Rate)
        {
            try
            {
                Logger.DebugFormat("Создаем тариф {0}", Rate);
                Rate result = dataService.Create<Rate>(Constants.RATE_OBJECT_TYPE_NAME, Rate);
                Logger.InfoFormat("Создан новый тариф {0}", result);

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["Rate"] = result;
                eventService.EvokeEvent(Constants.RATE_CREATED_EVENT_NAME, eventData);

                return result;
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при создании тарифа: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при создании тарифа", ex2);
                throw new Exception("Ошибка при создании тарифа");
            }
        }

        public void SaveRate(Rate Rate)
        {
            try
            {
                Rate tmp = GetRateById(Rate.Id);
                if (tmp == null)
                    throw new ServerDefinedException("Нет тарифа с таким Id");

                Logger.DebugFormat("Сохраняем тариф {0}", Rate);
                dataService.Save(Constants.RATE_OBJECT_TYPE_NAME, Rate);
                Logger.InfoFormat("Тариф сохранен {0}", Rate);

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["Rate"] = Rate;
                eventService.EvokeEvent(Constants.RATE_CHANGED_EVENT_NAME, eventData);
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при сохранении тарифа: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при сохранении тарифа", ex2);
                throw new Exception("Ошибка при сохранении тарифа");
            }
        }

        public void DeleteRate(Rate Rate)
        {
            try
            {
                Logger.DebugFormat("Удаляем тариф {0}", Rate);
                dataService.Delete(Constants.RATE_OBJECT_TYPE_NAME, Rate);
                Logger.InfoFormat("Тариф {0} удален", Rate.Id);

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["RateId"] = Rate.Id;
                eventService.EvokeEvent(Constants.RATE_DELETED_EVENT_NAME, eventData);
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при удалении тарифа", ex2);
                throw new ServerDefinedException("Тариф нельзя удалить, т.к. он привязан к другим сущностям.");
            }
        }



        public Payment CreatePayment(Payment Payment)
        {
            try
            {
                Logger.DebugFormat("Создаем платеж {0}", Payment);
                Payment result = dataService.Create<Payment>(Constants.PAYMENT_OBJECT_TYPE_NAME, Payment);
                Logger.InfoFormat("Создан новый платеж {0}", result);

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["Payment"] = result;
                eventService.EvokeEvent(Constants.PAYMENT_ADDED_EVENT_NAME, eventData);

                return result;
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при создании платежа: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при создании платежа", ex2);
                throw new Exception("Ошибка при создании платежа");
            }
        }

        public IEnumerable<Payment> GetPayments(IDictionary<string, object> Filter)
        {
            return dataService.GetList<Payment>(Constants.PAYMENT_OBJECT_TYPE_NAME, Filter);
        }

        public IEnumerable<Payment> GetSessionPayments(long SessionId)
        {
            IDictionary<string, object> filter = new Dictionary<string, object>();
            filter["SessionId"] = SessionId;
            return GetPayments(filter);
        }

        public int GetDebt(Rate Rate, DateTime DateFrom, DateTime DateTo)
        {
            return DebtCalculators[Rate.Type].GetDebt(Rate, DateFrom, DateTo);
        }
    }
}
