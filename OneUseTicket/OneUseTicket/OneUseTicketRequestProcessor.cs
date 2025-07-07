using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.Session;
using Hermes.Parking.Server.Equipment;

namespace Hermes.Parking.Server.OneUseTicket
{
    public class OneUseTicketRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private ISessionManager sm;
        private IEquipmentManager equipmentManager;
        private Hermes.Parking.Server.SerializationService.ISerializationService ss;

        public OneUseTicketRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.sm = Context.GetService<ISessionManager>(Session.Constants.SESSION_MANAGER_NAME);
            this.equipmentManager = Context.GetService<IEquipmentManager>(Equipment.Constants.EQUIPMENT_MANAGER_NAME);
            this.ss = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
        }

        public string GetName() { return Constants.ONE_USE_TICKET_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "GetNewOneUseTicket":
                    return GetNewOneUseTicket(Request.Data, Response.Data);

                case "Arrive":
                    return Arrive(Request.Data, Response.Data);

                case "Leave":
                    return Leave(Request.Data, Response.Data);

                case "Depart":
                    return Depart(Request.Data, Response.Data);

                case "AskForDepart":
                    return AskForDepart(Request.Data, Response.Data);

                default:
                    return true;
            }
        }


        private bool GetNewOneUseTicket(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            int rateId = GetRateId(InputData);
            Session.Session session = sm.CreateSession(TicketType.OneUseTicket, rateId);
            OutputData["SessionId"] = session.Id;
            OutputData["CreateTime"] = session.CreateTime;
            OutputData["TicketNumber"] = session.TicketNumber;
            return true;
        }

        // метод лишь добавляет инфу по сессии, чтобы обработчик сессии уже сделал все, что нужно
        private bool Arrive(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            TryGetSessionId(InputData);
            return true;
        }

        // метод лишь добавляет инфу по сессии, чтобы обработчик сессии уже сделал все, что нужно
        private bool Leave(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            TryGetSessionId(InputData);
            return true;
        }

        // метод лишь добавляет инфу по сессии, чтобы обработчик сессии уже сделал все, что нужно
        private bool Depart(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            TryGetSessionId(InputData);
            return true;
        }

        // метод лишь добавляет инфу по сессии, чтобы обработчик сессии уже сделал все, что нужно
        private bool AskForDepart(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            TryGetSessionId(InputData);
            return true;
        }

        // при наличии номера билета добавляет в InputData идентификатор сесси - для дальнейшей обработки
        private void TryGetSessionId(IDictionary<string, object> InputData)
        {
            if (InputData != null && InputData.ContainsKey("TicketNumber"))
            {
                long ticketNumber;

                if (!long.TryParse(InputData["TicketNumber"].ToString(), out ticketNumber))
                    throw new ServerDefinedException("Неправильный формат номера билета");

                Session.Session s = sm.GetCurrentSessionByTicketNumber(ticketNumber);

                if (s == null)
                    throw new ServerDefinedException("Открытой сессии с таким билетом не существует");

                InputData["SessionId"] = s.Id;
            }
        }

        private Device ParseGate(IDictionary<string, object> InputData)
        {
            int gateId;

            if (!InputData.ContainsKey("GateId"))
                throw new ServerDefinedException("Необходимо указать идентификатор стойки въезда");

            if (!int.TryParse(InputData["GateId"].ToString(), out gateId))
                throw new ServerDefinedException("Неправильный формат идентификатора стойки");

            Device gate = equipmentManager.GetDeviceById(gateId);
            if (gate == null)
                throw new ServerDefinedException("Указана несуществующая стойка въезда");

            if (gate.Type != DeviceType.GateTerminal)
                throw new ServerDefinedException("В качестве стойки указано иное оборудование");

            return gate;
        }

        private Device GetZoneOut(IDictionary<string, object> InputData)
        {
            Device gate = ParseGate(InputData);
            if (string.IsNullOrEmpty(gate.Config))
                throw new ServerDefinedException("У стойки нет конфига");
            GateConfig gateConfig = ss.Deserialize<GateConfig>(gate.Config);
            if (!gateConfig.ZoneOutId.HasValue || gateConfig.ZoneOutId.Value == 0)
                throw new ServerDefinedException("У стойки не указана зона выезда");

            Device zone = equipmentManager.GetDeviceById(gateConfig.ZoneOutId.Value);

            if (zone == null)
                throw new ServerDefinedException("У стойки указана несуществующая зона выезда");

            return zone;
        }

        private int GetRateId(IDictionary<string, object> InputData)
        {
            if (InputData == null)
                throw new ServerDefinedException("Нет входных параметров запроса");

            if (InputData.ContainsKey("RateId"))
                return ParseRateId(InputData);

            if (InputData.ContainsKey("GateId"))
            {
                Device zone = GetZoneOut(InputData);
                if (string.IsNullOrEmpty(zone.Config))
                    throw new ServerDefinedException("При создании билета указана зона въезда, не имеющая конфига");
                ZoneConfig zoneConfig = ss.Deserialize<ZoneConfig>(zone.Config);
                if (!zoneConfig.RateId.HasValue)
                    throw new ServerDefinedException("При создании билета указана зона въезда, не имеющая тарифа");

                return zoneConfig.RateId.Value;
            }

            throw new ServerDefinedException("Необходимо указать либо тариф, либо стойку въезда");
        }

        private int ParseRateId(IDictionary<string, object> InputData)
        {
            int rateId;

            if (!InputData.ContainsKey("RateId"))
                throw new ServerDefinedException("Необходимо указать идентификатор тарифа");

            if (!int.TryParse(InputData["RateId"].ToString(), out rateId))
                throw new ServerDefinedException("Неправильный формат идентификатора тарифа");

            return rateId;
        }
    }
}
