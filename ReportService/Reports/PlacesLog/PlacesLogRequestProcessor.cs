using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.Equipment;
using Hermes.Parking.Server.DataService;

namespace Hermes.Parking.Server.Reports.PlacesLog
{
    public class PlacesLogRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private IEquipmentManager equipmentManager;
        private IDataService dataService;
        private Hermes.Parking.Server.SerializationService.ISerializationService ss;

        public PlacesLogRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.ss = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
            this.equipmentManager = Context.GetService<IEquipmentManager>(Hermes.Parking.Server.Equipment.Constants.EQUIPMENT_MANAGER_NAME);
            this.dataService = Context.GetService<IDataService>(Hermes.Parking.Server.DataService.Constants.DATA_SERVICE_NAME);
        }

        public string GetName() { return Constants.PLACESLOG_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "Arrive":
                    return Arrive(Request.Data, Response.Data);

                case "Depart":
                    return Depart(Request.Data, Response.Data);

                case "CorrectPlaces":
                    return CorrectPlaces(Request.Data, Response.Data);

                case "GetPlacesLogReport":
                    return GetPlacesLogReport(Request.Data, Response.Data);

                default:
                    return true;
            }
        }

        private bool Arrive(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            Device zone = GetZoneOut(InputData);
            if (string.IsNullOrEmpty(zone.Config))
                throw new ServerDefinedException("У зоны въезда нет конфига");
            ZoneConfig zoneConfig = ss.Deserialize<ZoneConfig>(zone.Config);


            IDictionary<string, object> filter = new Dictionary<string, object>();
            filter["ZoneId"] = zone.Id;
            PlacesLog lastLogByZone = dataService.Get<PlacesLog>(Constants.PLACESLOG_OBJECT_TYPE_NAME, filter);

            PlacesLog placesLog = new PlacesLog()
            {
                ZoneId = zone.Id,
                PlacesCount = zoneConfig.PlacesNumber,
                PlacesOccupied = (lastLogByZone == null) ? 1 : lastLogByZone.PlacesOccupied + 1
            };

            dataService.Create<PlacesLog>(Constants.PLACESLOG_OBJECT_TYPE_NAME, placesLog);

            return true;
        }

        private bool Depart(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            Device zone = GetZoneOut(InputData);
            if (string.IsNullOrEmpty(zone.Config))
                throw new ServerDefinedException("У зоны въезда нет конфига");
            ZoneConfig zoneConfig = ss.Deserialize<ZoneConfig>(zone.Config);

            IDictionary<string, object> filter = new Dictionary<string, object>();
            filter["ZoneId"] = zone.Id;
            PlacesLog lastLogByZone = dataService.Get<PlacesLog>(Constants.PLACESLOG_OBJECT_TYPE_NAME, filter);

            PlacesLog placesLog = new PlacesLog()
            {
                ZoneId = zone.Id,
                PlacesCount = zoneConfig.PlacesNumber,
                PlacesOccupied = (lastLogByZone == null) ? 0 : lastLogByZone.PlacesOccupied - 1
            };

            dataService.Create<PlacesLog>(Constants.PLACESLOG_OBJECT_TYPE_NAME, placesLog);

            return true;
        }

        private bool CorrectPlaces(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null || !InputData.ContainsKey("PlacesLogs"))
                throw new ServerDefinedException("Не указаные входные данные коррекции");

            List<PlacesLog> logs = ParsePlacesLogs(InputData["PlacesLogs"].ToString());

            foreach (PlacesLog placesLog in logs)
            {
                Device zone = equipmentManager.GetDeviceById(placesLog.ZoneId);
                if (zone == null)
                    throw new ServerDefinedException(string.Format("При коррекции указана несуществующая зона {0}", placesLog.ZoneId));
            }

            foreach (PlacesLog placesLog in logs)
            {
                dataService.Create<PlacesLog>(Constants.PLACESLOG_OBJECT_TYPE_NAME, placesLog);
            }

            return true;
        }

        public bool GetPlacesLogReport(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<PlacesLog> placesLogs = dataService.GetList<PlacesLog>(Constants.PLACESLOG_OBJECT_TYPE_NAME, InputData);
            OutputData["PlacesLogReport"] = ss.Serialize(placesLogs);
            return true;
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

        private List<PlacesLog> ParsePlacesLogs(string JSON)
        {
            try
            {
                List<PlacesLog> result = ss.Deserialize<List<PlacesLog>>(JSON);
                if (result == null)
                    throw new ServerDefinedException("Неправильный формат входных данных коррекции");
                return result;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Не удалось распарсить входные данные коррекции", ex);
                throw new ServerDefinedException("Неправильный формат входных данных коррекции");
            }
        }
    }
}
