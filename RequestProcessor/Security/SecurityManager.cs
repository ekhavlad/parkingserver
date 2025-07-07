using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using Hermes.Parking.Server.EventService;
using Hermes.Parking.Server.SerializationService;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class SecurityManager : BaseService, ISecurityManager
    {
        private IDataService dataService;
        private IEventService eventService;

        private Dictionary<string, ServerRequest> requests = new Dictionary<string, ServerRequest>();
        private Dictionary<int, UserRole> roles = new Dictionary<int, UserRole>();
        private Dictionary<int, User> users = new Dictionary<int, User>();

        private object locker = new object();

        public override string GetName() { return Constants.SECURITY_MANAGER_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            this.dataService = Context.GetService<IDataService>(Hermes.Parking.Server.DataService.Constants.DATA_SERVICE_NAME);
            this.eventService = Context.GetService<IEventService>(Hermes.Parking.Server.EventService.Constants.EVENT_SERVICE_NAME);
        }

        public override void OnStart()
        {
            requests = dataService.GetList<ServerRequest>(Constants.REQUEST_OBJECT_TYPE_NAME, null).ToDictionary(x => x.Name, x => x);
            roles = dataService.GetList<UserRole>(Constants.USER_ROLE_OBJECT_TYPE_NAME, null).ToDictionary(x => x.Id, x => x);
            users = dataService.GetList<User>(Constants.USER_OBJECT_TYPE_NAME, null).ToDictionary(x => x.Id, x => x);
        }


        public User CreateUser(User User)
        {
            try
            {
                lock (locker)
                {
                    Logger.DebugFormat("Создаем пользователя {0}", User);

                    if (string.IsNullOrWhiteSpace(User.Login))
                        throw new ServerDefinedException("Нельзя создать пользователя с пустым логином");

                    if (users.Count(u => u.Value.Login == User.Login) > 0)
                        throw new ServerDefinedException(string.Format("Пользователь с логином '{0}' уже существует", User.Login));

                    if (User.Password == null)
                        User.Password = "";

                    if (User.Roles == null)
                        User.Roles = new List<int>();

                    User.Roles = User.Roles.Where(r => roles.Select(x => x.Key).Contains(r)).ToList();

                    User user = dataService.Create<User>(Constants.USER_OBJECT_TYPE_NAME, User);

                    // добавляем пользователя в список только тогда, когда все прошло успешно
                    users.Add(user.Id, user);

                    Logger.InfoFormat("Создан новый пользователь {0}", User);

                    // кидаем событие создания пользователя
                    Dictionary<string, object> eventData = new Dictionary<string, object>();
                    eventData["User"] = user;
                    eventService.EvokeEvent(Constants.USER_CREATED_EVENT_NAME, eventData);

                    // возвращаем клона из списка, чтобы пользователей в списке можно было менять только специальными зарпосами (например, Save)
                    return (User)user.Clone();
                }
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при создании пользователя: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при создании пользователя", ex2);
                throw new Exception("Ошибка при создании пользователя");
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            lock (locker)
            {
                return users.Select(x => (User)x.Value.Clone()).ToList();
            }
        }

        public void SaveUser(User User)
        {
            try
            {
                lock (locker)
                {
                    Logger.DebugFormat("Сохраняем пользователя {0}", User);

                    if (string.IsNullOrWhiteSpace(User.Login))
                        throw new ServerDefinedException("Нельзя создать пользователя с пустым логином");

                    if (users.Count(u => u.Key != User.Id && u.Value.Login == User.Login) > 0)
                        throw new ServerDefinedException(string.Format("Пользователь с логином '{0}' уже существует", User.Login));

                    if (!users.ContainsKey(User.Id))
                        throw new ServerDefinedException("Нет пользователя с таким Id");

                    if (User.Roles == null)
                        User.Roles = new List<int>();

                    // чистим роли
                    User.Roles = User.Roles.Where(r => roles.Select(x => x.Key).Contains(r)).ToList();
                    // сохраняем в БД
                    dataService.Save(Constants.USER_OBJECT_TYPE_NAME, User);
                    // сохраняем в онлайн список
                    users[User.Id] = (User)User.Clone();

                    Logger.InfoFormat("Пользователь сохранен {0}", User);

                    // кидаем событие обновления пользователя
                    Dictionary<string, object> eventData = new Dictionary<string, object>();
                    eventData["User"] = User;
                    eventService.EvokeEvent(Constants.USER_CHANGED_EVENT_NAME, eventData);
                }
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Ошибка сохранения пользователя: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка при сохранении пользователя", ex);
                throw new Exception("Ошибка при сохранении пользователя");
            }
        }

        public void DeleteUser(User User)
        {
            try
            {
                lock (locker)
                {
                    Logger.DebugFormat("Удаляем пользователя id:{0}", User.Id);

                    if (User.Id == Constants.ROOT_ID)
                        throw new ServerDefinedException("Нельзя удалить главного администратора");

                    if (!users.ContainsKey(User.Id))
                        throw new ServerDefinedException("Нет пользователя с таким Id");

                    User user = users[User.Id];

                    if (user.Roles == null)
                        user.Roles = new List<int>();

                    try
                    {
                        dataService.Delete(Constants.USER_OBJECT_TYPE_NAME, user);
                        users.Remove(user.Id);
                        Logger.InfoFormat("Пользователь с id:{0} удален", user.Id);

                        // кидаем событие удаления пользователя
                        Dictionary<string, object> eventData = new Dictionary<string, object>();
                        eventData["UserId"] = user.Id;
                        eventService.EvokeEvent(Constants.USER_DELETED_EVENT_NAME, eventData);
                    }
                    catch
                    {
                        if (user.IsActive)
                        {
                            User.IsActive = false;
                            dataService.Save(Constants.USER_OBJECT_TYPE_NAME, user);
                            user.IsActive = false;
                            Logger.InfoFormat("Пользователя {0} нельзя удалить, т.к. он привязан к другим сущностям. Пользователь деактивирован.", user.Id);

                            // кидаем событие изменения пользователя
                            Dictionary<string, object> eventData = new Dictionary<string, object>();
                            eventData["User"] = user.Id;
                            eventService.EvokeEvent(Constants.USER_CHANGED_EVENT_NAME, eventData);

                            throw new ServerDefinedException("Пользователя нельзя удалить, т.к. он привязан к другим сущностям. Пользователь деактивирован.");
                        }
                        else
                        {
                            throw new ServerDefinedException("Пользователя нельзя удалить, т.к. он привязан к другим сущностям.");
                        }
                    }
                }
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Ошибка удаления пользователя: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка при удалении пользователя", ex);
                throw new Exception("Ошибка при удалении пользователя");
            }
        }


        public UserRole CreateUserRole(UserRole Role)
        {
            try
            {
                lock (locker)
                {
                    Logger.DebugFormat("Создаем роль {0}", Role);

                    if (string.IsNullOrWhiteSpace(Role.Name))
                        throw new ServerDefinedException(string.Format("Нельзя создать роль с пустым названием", Role.Name));

                    if (roles.Count(u => u.Value.Name == Role.Name) > 0)
                        throw new ServerDefinedException(string.Format("Роль с названием '{0}' уже существует", Role.Name));

                    if (Role.Requests == null)
                        Role.Requests = new List<string>();

                    Role.Requests = Role.Requests.Where(r => requests.Select(x => x.Key).Contains(r)).ToList();

                    UserRole role = dataService.Create<UserRole>(Constants.USER_ROLE_OBJECT_TYPE_NAME, Role);

                    // добавляем роль в список только тогда, когда все прошло успешно
                    roles.Add(role.Id, role);

                    Logger.InfoFormat("Создана роль {0}", role);

                    // кидаем событие создания роли
                    Dictionary<string, object> eventData = new Dictionary<string, object>();
                    eventData["UserRole"] = role;
                    eventService.EvokeEvent(Constants.USER_ROLE_CREATED_EVENT_NAME, eventData);

                    // возвращаем клона из списка, чтобы роль в списке можно было менять только специальными зарпосами (например, Save)
                    return (UserRole)role.Clone();
                }
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Ошибка создания роли: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка при создании роли", ex);
                throw new Exception("Ошибка при создании роли");
            }
        }

        public IEnumerable<UserRole> GetAllUserRoles()
        {
            lock (locker)
            {
                return roles.Select(x => (UserRole)x.Value.Clone()).ToList();
            }
        }

        public void SaveUserRole(UserRole Role)
        {
            try
            {
                lock (locker)
                {
                    Logger.DebugFormat("Сохраняем роль {0}", Role);

                    if (string.IsNullOrWhiteSpace(Role.Name))
                        throw new ServerDefinedException(string.Format("Нельзя создать роль с пустым названием", Role.Name));

                    if (roles.Count(u => u.Key != Role.Id && u.Value.Name == Role.Name) > 0)
                        throw new ServerDefinedException(string.Format("Роль с названием '{0}' уже существует", Role.Name));

                    if (!roles.ContainsKey(Role.Id))
                        throw new ServerDefinedException("Нет роли с таким Id");

                    UserRole role = roles[Role.Id];

                    if (Role.Requests == null)
                        Role.Requests = new List<string>();

                    Role.Requests = Role.Requests.Where(r => requests.Select(x => x.Key).Contains(r)).ToList();

                    dataService.Save(Constants.USER_ROLE_OBJECT_TYPE_NAME, Role);
                    role.Name = Role.Name;
                    role.Requests = Role.Requests.ToList();

                    Role = (UserRole)role.Clone();

                    Logger.InfoFormat("Роль сохранена {0}", role);

                    // кидаем событие обновления роли
                    Dictionary<string, object> eventData = new Dictionary<string, object>();
                    eventData["UserRole"] = role;
                    eventService.EvokeEvent(Constants.USER_ROLE_CHANGED_EVENT_NAME, eventData);
                }
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Ошибка сохранения роли: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка при сохранении роли", ex);
                throw new Exception("Ошибка при сохранении роли");
            }
        }

        public void DeleteUserRole(UserRole Role)
        {
            try
            {
                lock (locker)
                {
                    Logger.DebugFormat("Удаляем роль id:{0}", Role.Id);

                    if (!roles.ContainsKey(Role.Id))
                        throw new ServerDefinedException("Нет роли с таким Id");

                    UserRole role = roles[Role.Id];

                    dataService.Delete(Constants.USER_ROLE_OBJECT_TYPE_NAME, Role);
                    roles.Remove(role.Id);

                    Logger.InfoFormat("Роль удалена id:{0}", Role.Id);

                    // кидаем событие создания роли
                    Dictionary<string, object> eventData = new Dictionary<string, object>();
                    eventData["UserRoleId"] = role.Id;
                    eventService.EvokeEvent(Constants.USER_ROLE_DELETED_EVENT_NAME, eventData);
                }
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Ошибка удаления роли: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка при удалении роли", ex);
                throw new Exception("Ошибка при удалении роли");
            }
        }


        public IEnumerable<ServerRequest> GetAllRequests()
        {
            return requests.Select(x => (ServerRequest)x.Value.Clone()).ToList();
        }

        public IEnumerable<ServerRequest> GetAvailableRequests(User User)
        {
            // админу все доступно
            if (User.Id == Constants.ROOT_ID)
                return GetAllRequests();

            List<string> tmp = new List<string>();
            tmp.Add("GetAvailableRequests");

            foreach (int roleId in User.Roles)
                foreach (string req in roles[roleId].Requests)
                    tmp.Add(req);

            List<ServerRequest> result = new List<ServerRequest>();
            foreach (string req in tmp.Distinct())
                result.Add(requests[req]);

            return result;
        }

        public ServerRequest GetRequestById(int Id)
        {
            lock (locker)
            {
                if (requests.Count(r => r.Value.Id == Id) == 0)
                    return null;
                else
                    return (ServerRequest)requests.Single(r => r.Value.Id == Id).Value.Clone();
            }
        }

        public ServerRequest GetRequestByName(string Name)
        {
            lock (locker)
            {
                if (!requests.ContainsKey(Name))
                    return null;
                else
                    return (ServerRequest)requests[Name].Clone();
            }
        }


        public bool IsRequestAvailable(int UserId, string RequestName)
        {
            lock (locker)
            {
                if (!users.ContainsKey(UserId))
                    return false;

                if (!requests.ContainsKey(RequestName))
                    return false;

                if (UserId == Constants.ROOT_ID || RequestName == "GetAvailableRequests")
                    return true;

                User user = users[UserId];

                foreach (int roleId in user.Roles)
                    if (roles[roleId].Requests.Contains(RequestName))
                        return true;

                return false;
            }
        }


        public User Login(string Login, string Password)
        {
            lock (locker)
            {
                List<User> tmp = users.Where(u => u.Value.Login == Login && u.Value.Password == Password && (u.Value.IsActive || u.Value.Id == Constants.ROOT_ID)).Select(x => x.Value).ToList();

                if (tmp.Count == 0)
                    throw new ServerDefinedException("Ошибка авторизации - проверьте правильность ввода логина и пароля");

                User user = tmp.First();

                return user;
            }
        }
    }
}
