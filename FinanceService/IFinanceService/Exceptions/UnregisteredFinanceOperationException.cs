using System;

namespace Hermes.Parking.Server.FinanceService
{
    public class UnregisteredFinanceOperationException : Exception
    {
        public UnregisteredFinanceOperationException(string OperationType)
            : base(string.Format("Call to unregistered finance operation '{0}'", OperationType))
        {
        }
    }
}
