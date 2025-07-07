namespace Hermes.Parking.Server.Session
{
    public static class Constants
    {
        public const int GRACE_PERIOD = 15;

        public const string SESSION_MANAGER_NAME = "SessionManager";
        public const string SESSION_DATA_PROVIDER_NAME = "SessionDataProvider";
        public const string SESSION_REQUEST_PROCESSOR_NAME = "SessionRequestProcessor";

        public const string SESSION_OBJECT_TYPE_NAME = "Session";
        public const string SESSION_CREATED_EVENT_NAME = "SessionCreated";
        public const string SESSION_CHANGED_EVENT_NAME = "SessionChanged";

        public const string BILL_OBJECT_TYPE_NAME = "SessionBill";
    }
}