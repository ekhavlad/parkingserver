using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Parking.Server.EventService;

using System.Threading;

namespace Hermes.Parking.Server.ServerAPI
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ServerAPI : IServerAPI
    {
        private static ILogger Logger;
        private static IEventService eventService;
        private static IPrimaryRequestProcessor request;
        private static ISecurityManager security;
        private static Hermes.Parking.Server.SerializationService.ISerializationService serializationService;

        private static object locker = new object();
        private static Dictionary<int, object> lockers = new Dictionary<int, object>();
        private static Dictionary<User, IServerAPICallback> usersOnline = new Dictionary<User, IServerAPICallback>();

        private User user = null;

        private static Thread pingThread = new Thread(PingThread);

        public static void Init(IContext Context)
        {
            Logger = Context.GetService<ILogger>("Logger");
            eventService = Context.GetService<IEventService>(Hermes.Parking.Server.EventService.Constants.EVENT_SERVICE_NAME);
            eventService.OnEvent += OnEvent;
            request = Context.GetService<IPrimaryRequestProcessor>(Hermes.Parking.Server.RequestProcessor.Constants.PRIMARY_REQUEST_PROCESSOR_NAME);
            security = Context.GetService<ISecurityManager>(Hermes.Parking.Server.RequestProcessor.Constants.SECURITY_MANAGER_NAME);
            serializationService = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
            pingThread.Start();
        }

        public static void Stop()
        {
            pingThread.Abort();
        }


        public Response Login(string Login, string Password)
        {
            try
            {
                lock (locker)
                {
                    if (user != null)
                    {
                        if (usersOnline.ContainsKey(user))
                            usersOnline.Remove(user);

                        if (lockers.ContainsKey(user.Id))
                            lockers.Remove(user.Id);

                        user = null;
                    }

                    Logger.DebugFormat("Пользователь '{0}' пытается войти в систему", Login);

                    user = security.Login(Login, Password);

                    if (user == null)
                    {
                        Logger.InfoFormat("Пользователю '{0}' не удалось войти в систему", Login);
                        return new Response(false, "Ошибка авторизации");
                    }

                    lockers[user.Id] = new object();
                }
                lock (lockers[user.Id])
                {
                    Logger.InfoFormat("Пользователь {0} ({1}) вошел в систему", user.Id, user.Login);

                    IServerAPICallback callback = OperationContext.Current.GetCallbackChannel<IServerAPICallback>();

                    if (usersOnline.ContainsKey(user))
                    {
                        usersOnline.Remove(user);
                        usersOnline.Add(user, callback);
                        Logger.DebugFormat("Пользователь {0} ({1}) был переподписан на колбэки", user.Id, user.Login);
                    }
                    else
                    {
                        usersOnline.Add(user, callback);
                        Logger.DebugFormat("Пользователь {0} ({1}) был подписан на колбэки", user.Id, user.Login);
                    }

                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data["UserId"] = user.Id;
                    eventService.EvokeEvent(Constants.USER_GOT_ONLINE_CALLBACK, data);

                    return new Response(true);
                }
            }
            catch (ServerDefinedException ex)
            {
                Logger.ErrorFormat("Ошибка входа пользователя в систему", ex);
                return new Response(false, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка входа пользователя в систему", ex);
                return new Response(false, "Ошибка входа в систему");
            }
        }

        public Response Logout()
        {
            try
            {
                Logger.Debug("Пользователь пытается выйти из системы");

                if (user == null)
                {
                    Logger.Info("Пользователь не залогинен - попытка выхода провалилась");
                    return new Response(false, "Пользователь не авторизован");
                }

                lock (lockers[user.Id])
                {

                    int userId = user.Id;
                    string login = user.Login;

                    if (usersOnline.ContainsKey(user))
                        usersOnline.Remove(user);

                    if (lockers.ContainsKey(user.Id))
                        lockers.Remove(user.Id);

                    Logger.DebugFormat("Пользователь {0} ({1}) отписан от колбэков", user.Id, user.Login);
                    user = null;
                    Logger.InfoFormat("Пользователь {0} ({1}) вышел из системы", userId, login);

                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data["UserId"] = userId;
                    eventService.EvokeEvent(Constants.USER_GOT_OFFLINE_CALLBACK, data);

                    return new Response(true);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка выхода пользователя из системы", ex);
                return new Response(false, "Ошибка выхода из системы");
            }
        }

        public Response ProcessRequest(Request Request)
        {
            try
            {
                lock (lockers[user.Id])
                {
                    return request.ProcessRequest(user, Request);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Fatal error while processing API request!", ex);
                return new Response(false, "Ошибка обработки запроса");
            }
        }


        private static void OnEvent(string EventName, IDictionary<string, object> Data)
        {
            Thread t = new Thread(() => EventThread(EventName, Data));
            t.Start();
        }

        private static void EventThread(string EventName, IDictionary<string, object> Data)
        {
            try
            {
                List<Task> tasks = new List<Task>();
                lock (locker)
                {
                    foreach (var x in Data.ToList())
                    {
                        Data[x.Key] = serializationService.Serialize(Data[x.Key]);
                    }

                    foreach (var client in usersOnline.Where(x => security.IsRequestAvailable(x.Key.Id, EventName)))
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            if (!SendCommandToUser(client.Key, EventName, Data))
                            {
                                Logger.WarnFormat("'{0}' is offline - event send error", client.Key);
                                lock (locker)
                                    usersOnline.Remove(client.Key);
                            }
                        }));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Unhandled exception while processing event thread in API host", ex);
                return;
            }
        }

        private static bool SendCommandToUser(User User, string CommandName, IDictionary<string, object> Data)
        {
            try
            {
                lock (lockers[User.Id])
                    usersOnline[User].ProcessCommand(new CallbackCommand(CommandName, Data));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка отправки команды пользователю", ex);
                return false;
            }
        }


        private static void PingUsers()
        {
            try
            {
                List<Task> tasks = new List<Task>();

                lock (locker)
                {
                    foreach (var client in usersOnline)
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            if (!PingUser(client.Key))
                            {
                                Logger.WarnFormat("'{0}' is offline - no ping", client.Key);
                                usersOnline.Remove(client.Key);

                                Dictionary<string, object> data = new Dictionary<string, object>();
                                data["UserId"] = client.Key;
                                eventService.EvokeEvent("UserGotOffline", data);
                            }
                            else
                            {
                                //Logger.InfoFormat("{0} is online", client.Key);
                            }
                        }));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка пинга пользователей", ex);
                return;
            }
        }

        private static bool PingUser(User User)
        {
            try
            {
                lock (lockers[User.Id])
                {
                    usersOnline[User].Ping();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Ошбика пинга пользователя {0} ({1})", ex, User.Id, User.Login);
                return false;
            }
        }

        private static void PingThread()
        {
            while (true)
            {
                PingUsers();
                Thread.Sleep(1000);
            }
        }
    }
}
