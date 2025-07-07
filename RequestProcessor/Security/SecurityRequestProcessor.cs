using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Core.Interfaces;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class SecurityRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private ISecurityManager securityManager;
        private Hermes.Parking.Server.SerializationService.ISerializationService serializationService;

        public SecurityRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.securityManager = Context.GetService<ISecurityManager>(Constants.SECURITY_MANAGER_NAME);
            this.serializationService = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
        }


        public string GetName() { return Constants.SECURITY_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Hermes.Parking.Server.RequestProcessor.Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "GetAllUsers":
                    return GetAllUsers(Request.Data, Response.Data);
                case "CreateUser":
                    return CreateUser(Request.Data, Response.Data);
                case "UpdateUser":
                    return UpdateUser(Request.Data, Response.Data);
                case "DeleteUser":
                    return DeleteUser(Request.Data, Response.Data);
                case "ChangePassword":
                    return ChangePassword(User, Request.Data, Response.Data);

                case "CreateUserRole":
                    return CreateUserRole(Request.Data, Response.Data);
                case "GetAllUserRoles":
                    return GetAllUserRoles(Request.Data, Response.Data);
                case "UpdateUserRole":
                    return UpdateUserRole(Request.Data, Response.Data);
                case "DeleteUserRole":
                    return DeleteUserRole(Request.Data, Response.Data);

                case "GetAllRequests":
                    return GetAllRequests(Request.Data, Response.Data);

                case "GetAvailableRequests":
                    return GetAvailableRequests(User, Request.Data, Response.Data);

                default:
                    return true;
            }
        }

        private User ParseUser(string JSON)
        {
            try
            {
                User result = serializationService.Deserialize<User>(JSON);
                if (result == null)
                    throw new ServerDefinedException("Неправильный формат входных данных пользователя");
                return result;
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Не удалось распарсить пользователя: {0}", ex.Message);
                throw ex;
            }
            catch(Exception ex)
            {
                Logger.WarnFormat("Не удалось распарсить пользователя", ex);
                throw new ServerDefinedException("Неправильный формат входных данных пользователя");
            }
        }

        public bool CreateUser(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("User")) throw new ServerDefinedException("Нет поля User");
            User user = securityManager.CreateUser(ParseUser(InputData["User"].ToString()));
            OutputData["User"] = serializationService.Serialize(user);
            return true;
        }

        public bool GetAllUsers(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<User> users = securityManager.GetAllUsers();
            OutputData["Users"] = serializationService.Serialize(users);
            return true;
        }

        public bool UpdateUser(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("User")) throw new ServerDefinedException("Нет поля User");
            User user = ParseUser(InputData["User"].ToString());
            securityManager.SaveUser(user);
            OutputData["User"] = serializationService.Serialize(user);
            return true;
        }

        public bool DeleteUser(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("User")) throw new ServerDefinedException("Нет поля User");
            User user = ParseUser(InputData["User"].ToString());
            securityManager.DeleteUser(user);
            return true;
        }

        public bool ChangePassword(User User, IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Password")) throw new ServerDefinedException("Нет поля Password");
            string password = (string)InputData["Password"];
            User.Password = password;
            securityManager.SaveUser(User);
            return true;
        }


        private UserRole ParseUserRole(string JSON)
        {
            try
            {
                UserRole result = serializationService.Deserialize<UserRole>(JSON);
                if (result == null)
                    throw new ServerDefinedException("Неправильный формат входных данных роли пользователя");
                return result;
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Не удалось распарсить роль пользователя: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Не удалось распарсить роль пользователя", ex);
                throw new ServerDefinedException("Неправильный формат входных данных роли пользователя");
            }
        }

        public bool CreateUserRole(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("UserRole")) throw new ServerDefinedException("Нет поля UserRole");
            UserRole userRole = securityManager.CreateUserRole(ParseUserRole(InputData["UserRole"].ToString()));
            OutputData["UserRole"] = serializationService.Serialize(userRole);
            return true;
        }

        public bool GetAllUserRoles(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<UserRole> roles = securityManager.GetAllUserRoles();
            OutputData["UserRoles"] = serializationService.Serialize(roles);
            return true;
        }

        public bool UpdateUserRole(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("UserRole")) throw new ServerDefinedException("Нет поля UserRole");
            UserRole userRole = ParseUserRole(InputData["UserRole"].ToString());
            securityManager.SaveUserRole(userRole);
            OutputData["UserRole"] = serializationService.Serialize(userRole);
            return true;
        }

        public bool DeleteUserRole(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("UserRole")) throw new ServerDefinedException("Нет поля UserRole");
            UserRole userRole = ParseUserRole(InputData["UserRole"].ToString());
            securityManager.DeleteUserRole(userRole);
            return true;
        }


        public bool GetAllRequests(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<ServerRequest> requests = securityManager.GetAllRequests();
            OutputData["Requests"] = serializationService.Serialize(requests);
            return true;
        }

        public bool GetAvailableRequests(User User, IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<ServerRequest> requests = securityManager.GetAvailableRequests(User);
            OutputData["Requests"] = serializationService.Serialize(requests);
            return true;
        }
    }
}
