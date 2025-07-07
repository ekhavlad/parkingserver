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

namespace Hermes.Parking.Server.Reports.PassageLog
{
    public class PassageLogRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private IEquipmentManager equipmentManager;
        private IDataService dataService;
        private Hermes.Parking.Server.SerializationService.ISerializationService ss;

        public PassageLogRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.ss = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
            this.equipmentManager = Context.GetService<IEquipmentManager>(Hermes.Parking.Server.Equipment.Constants.EQUIPMENT_MANAGER_NAME);
            this.dataService = Context.GetService<IDataService>(Hermes.Parking.Server.DataService.Constants.DATA_SERVICE_NAME);
        }

        public string GetName() { return Constants.PASSAGELOG_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "Arrive":
                    return Arrive(Request.Data, Response.Data);

                case "Depart":
                    return Depart(Request.Data, Response.Data);

                case "GetPassageLogReport":
                    return GetPassageLogReport(Request.Data, Response.Data);

                default:
                    return true;
            }
        }

        private bool Arrive(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            Device gate = ParseGate(InputData);

            PassageLog passageLog = new PassageLog()
            {
                GateId = gate.Id
            };

            dataService.Create<PassageLog>(Constants.PASSAGELOG_OBJECT_TYPE_NAME, passageLog);

            return true;
        }

        private bool Depart(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            Device gate = ParseGate(InputData);

            PassageLog passageLog = new PassageLog()
            {
                GateId = gate.Id
            };

            dataService.Create<PassageLog>(Constants.PASSAGELOG_OBJECT_TYPE_NAME, passageLog);

            return true;
        }

        public bool GetPassageLogReport(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<PassageLog> passageLogs = dataService.GetList<PassageLog>(Constants.PASSAGELOG_OBJECT_TYPE_NAME, InputData);
            OutputData["PassageLogReport"] = ss.Serialize(passageLogs);
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
    }
}
