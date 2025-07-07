using System;
using System.Collections.Generic;

namespace Hermes.Parking.Server.FinanceService
{
    /// <summary>
    /// Задача сервиса - расчет задолженностей/балансов, а также обработка платежей.
    /// Сервис делегирует обработку операций последовательно нескольким обработчикам, умеющими обрабатывать соответствующие сущности.
    /// </summary>
    public interface IFinanceService
    {
        /// <summary>
        /// Список тарифных калькуляторов для разных типов тарифов
        /// </summary>
        IDictionary<RateType, IDebtCalculator> DebtCalculators { get; }


        /// <summary>
        /// Возвращает тариф
        /// </summary>
        Rate GetRateById(int Id);

        /// <summary>
        /// Возвращает все тарифы
        /// </summary>
        IEnumerable<Rate> GetAllRates();

        /// <summary>
        /// Создает тариф
        /// </summary>
        Rate CreateRate(Rate Rate);

        /// <summary>
        /// Обновляет тариф
        /// </summary>
        void SaveRate(Rate Rate);

        /// <summary>
        /// удаляет тариф
        /// </summary>
        void DeleteRate(Rate Rate);


        /// <summary>
        /// Создает платеж.
        /// </summary>
        Payment CreatePayment(Payment Payment);

        /// <summary>
        /// Возвращает список платежей по фильтру.
        /// </summary>
        IEnumerable<Payment> GetPayments(IDictionary<string, object> Filter);

        /// <summary>
        /// Возвращает список платежей по сессии.
        /// </summary>
        IEnumerable<Payment> GetSessionPayments(long SessionId);


        /// <summary>
        /// Считает общую задолженность по тарифу Type с DateFrom по DateTo
        /// </summary>
        /// <returns></returns>
        int GetDebt(Rate Rate, DateTime DateFrom, DateTime DateTo);
    }
}
