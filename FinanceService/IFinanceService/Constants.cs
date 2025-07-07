namespace Hermes.Parking.Server.FinanceService
{
    public static class Constants
    {
        public const string FINANCE_SERVICE_NAME = "FinanceService";
        public const string FINANCE_DATA_PROVIDER_NAME = "FinanceDataProvider";
        public const string FINANCE_SERVICE_REQUEST_PROCESSOR_NAME = "FinanceServiceRequestProcessor";

        public const string RATE_OBJECT_TYPE_NAME = "Rate";

        public const string RATE_CREATED_EVENT_NAME = "RateCreated";
        public const string RATE_CHANGED_EVENT_NAME = "RateChanged";
        public const string RATE_DELETED_EVENT_NAME = "RateDeleted";

        public const string PAYMENT_OBJECT_TYPE_NAME = "Payment";
        public const string PAYMENT_ADDED_EVENT_NAME = "PaymentAdded";
    }
}