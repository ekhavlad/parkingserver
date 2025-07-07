namespace Hermes.Parking.Server.RequestProcessor
{
    public static class Constants
    {
        /// <summary>
        /// Имя главного обработчика запросов по-умолчанию.
        /// </summary>
        public const string PRIMARY_REQUEST_PROCESSOR_NAME = "PrimaryRequestProcessor";


        public const int ROOT_ID = 1;
        public const int USER_TYPE_DICTIONARY_ID = 3;

        public const string SECURITY_MANAGER_NAME = "SecurityManager";
        public const string SECURITY_DATA_PROVIDER_NAME = "SecurityDataProvider";
        public const string SECURITY_REQUEST_PROCESSOR_NAME = "SecurityRequestProcessor";

        public const string REQUEST_OBJECT_TYPE_NAME = "Operation";
        public const string USER_ROLE_OBJECT_TYPE_NAME = "UserRole";
        public const string USER_OBJECT_TYPE_NAME = "User";

        public const string USER_CREATED_EVENT_NAME = "UserCreated";
        public const string USER_CHANGED_EVENT_NAME = "UserChanged";
        public const string USER_DELETED_EVENT_NAME = "UserDeleted";

        public const string USER_ROLE_CREATED_EVENT_NAME = "UserRoleCreated";
        public const string USER_ROLE_CHANGED_EVENT_NAME = "UserRoleChanged";
        public const string USER_ROLE_DELETED_EVENT_NAME = "UserRoleDeleted";

        public const string REQUEST_LOG_MANAGER_NAME = "RequestLogManager";
        public const string REQUEST_LOG_DATA_PROVIDER_NAME = "RequestLogDataProvider";
        public const string REQUEST_LOG_REQUEST_PROCESSOR_NAME = "RequestLogRequestProcessor";
        public const string REQUEST_LOG_OBJECT_TYPE_NAME = "RequestLog";
        public const string REQUEST_LOG_ADDED_EVENT_NAME = "RequestLogAdded";
    }
}