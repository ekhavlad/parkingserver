using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateModule;
using RateModule.ViewModel;

namespace Hermes.Parking.Server.FinanceService
{
    public class BaseDebtCalculator : IDebtCalculator
    {
        private static object locker = new object();
        private static Dictionary<string, RateCalculator> calculators = new Dictionary<string, RateCalculator>();

        public int GetDebt(Rate Rate, DateTime DateFrom, DateTime DateTo)
        {
            RateCalculator calculator;
            lock (locker)
            {
                if (calculators.ContainsKey(Rate.Config))
                {
                    calculator = calculators[Rate.Config];
                }
                else
                {
                    calculator = new RateCalculator();
                    calculator.ActiveRateSerializable = Rate.Config;
                    calculators[Rate.Config] = calculator;
                }
            }
            
            decimal tmp = calculator.CalculateTotal(DateFrom, DateTo);
            int result = (int)(tmp * 100);
            return result;
        }
    }
}
