using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Core.Interfaces;

namespace Hermes.Parking.Server.Session
{
    public class SessionRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private ISessionManager sm;
        private Hermes.Parking.Server.SerializationService.ISerializationService ss;

        public SessionRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.sm = Context.GetService<ISessionManager>(Constants.SESSION_MANAGER_NAME);
            this.ss = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
        }

        public string GetName() { return Constants.SESSION_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "GetSessions":
                    return GetSessions(Request.Data, Response.Data);

                case "SetSessionGraceTime":
                    return SetSessionGraceTime(Request.Data, Response.Data);

                case "RefreshSessionGracePeriod":
                    return RefreshSessionGracePeriod(Request.Data, Response.Data);

                case "Arrive":
                    return Arrive(Request.Data, Response.Data);

                case "Leave":
                    return Leave(Request.Data, Response.Data);

                case "Depart":
                    return Depart(Request.Data, Response.Data);

                case "AskForDepart":
                    return AskForDepart(Request.Data, Response.Data);

                case "CloseSession":
                    return CloseSession(Request.Data, Response.Data);

                default:
                    return true;
            }
        }

        private bool GetSessions(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<Session> d = sm.GetSessions(InputData);
            OutputData["Sessions"] = ss.Serialize(d);
            return true;
        }

        private bool SetSessionGraceTime(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            Session s = ParseSession(InputData);
            DateTime grace = DateTime.Now;

            if (!InputData.ContainsKey("GraceTime"))
                throw new ServerDefinedException("Необходимо указать GraceTime");

            if (!DateTime.TryParse(InputData["GraceTime"].ToString(), out grace))
                throw new ServerDefinedException("Неправильный формат GraceTime");

            sm.SetGraceTime(s, grace);
            OutputData["Session"] = ss.Serialize(s);
            return true;
        }

        private bool RefreshSessionGracePeriod(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            Session s = ParseSession(InputData);
            sm.RefreshGraceTime(s);
            OutputData["Session"] = ss.Serialize(s);
            return true;
        }

        private bool CloseSession(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            Session s = ParseSession(InputData);
            sm.Close(s);
            return true;
        }


        private bool Arrive(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            int gateId = ParseGateId(InputData);
            Session session = ParseSession(InputData);
            sm.Arrive(session, gateId);
            return true;
        }

        private bool Leave(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            Session session = ParseSession(InputData);
            sm.Cancel(session);
            return true;
        }

        private bool Depart(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            int gateId = ParseGateId(InputData);
            Session session = ParseSession(InputData);
            sm.Depart(session, gateId);
            return true;
        }

        private bool AskForDepart(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null)
                throw new ServerDefinedException("Нет входных параметров запроса");

            Session session = ParseSession(InputData);
            bool result = sm.IsDepartAvailable(session);
            OutputData["IsDepartAvailable"] = result;
            return true;
        }

        private Session ParseSession(IDictionary<string, object> InputData)
        {
            if (InputData == null)
                throw new ServerDefinedException("Нет входных параметров запроса");

            long sessionId;

            if (!InputData.ContainsKey("SessionId"))
                throw new ServerDefinedException("Необходимо указать идентификатор сессии");

            if (!long.TryParse(InputData["SessionId"].ToString(), out sessionId))
                throw new ServerDefinedException("Неправильный формат идентификатора сессии");

            Session s = sm.GetSessionById(sessionId);
            if (s == null)
                throw new ServerDefinedException("Сессии не существует");

            return s;
        }

        private int ParseGateId(IDictionary<string, object> InputData)
        {
            if (InputData == null)
                throw new ServerDefinedException("Нет входных параметров запроса");

            int gateId;

            if (!InputData.ContainsKey("GateId"))
                throw new ServerDefinedException("Необходимо указать идентификатор стойки въезда");
            
            if (!int.TryParse(InputData["GateId"].ToString(), out gateId))
                throw new ServerDefinedException("Неправильный формат идентификатора стойки");
            
            return gateId;
        }
    }
}
