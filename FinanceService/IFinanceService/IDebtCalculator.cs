using System;

namespace Hermes.Parking.Server.FinanceService
{
    public interface IDebtCalculator
    {
        int GetDebt(Rate Rate, DateTime DateFrom, DateTime DateTo);
    }
}
