using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Core.Interfaces;

namespace Hermes.Parking.Server.Equipment
{
    public class EquipmentRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private IEquipmentManager equipmentManager;
        private Hermes.Parking.Server.SerializationService.ISerializationService serializationService;

        public EquipmentRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.equipmentManager = Context.GetService<IEquipmentManager>(Constants.EQUIPMENT_MANAGER_NAME);
            this.serializationService = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
        }

        public string GetName() { return Constants.EQUIPMENT_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "GetAllDevices":
                    return GetAllDevices(Request.Data, Response.Data);
                case "RegisterDevice":
                    return RegisterDevice(Request.Data, Response.Data);
                case "UpdateDevice":
                    return UpdateDevice(Request.Data, Response.Data);
                case "DeleteDevice":
                    return DeleteDevice(Request.Data, Response.Data);

                case "AddDevicesStates":
                    return AddDevicesStates(Request.Data, Response.Data);
                case "GetLastDevicesStates":
                    return GetLastDevicesStates(Request.Data, Response.Data);

                case "AddDevicesEvents":
                    return AddDevicesEvents(Request.Data, Response.Data);
                case "GetDevicesEvents":
                    return GetDevicesEvents(Request.Data, Response.Data);

                default:
                    return true;
            }
        }


        private Device ParseDevice(string JSON)
        {
            try
            {
                Device result = serializationService.Deserialize<Device>(JSON);
                if (result == null)
                    throw new ServerDefinedException("Неправильный формат входных данных устройства");
                return result;
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Не удалось распарсить устройство: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Не удалось распарсить устройство", ex);
                throw new ServerDefinedException("Неправильный формат входных данных устройства");
            }
        }

        public bool RegisterDevice(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Device")) throw new ServerDefinedException("Нет поля Device");
            Device device = ParseDevice(InputData["Device"].ToString());
            equipmentManager.RegisterDevice(device);
            OutputData["Device"] = serializationService.Serialize(device);
            return true;
        }

        public bool GetAllDevices(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<Device> devices = equipmentManager.GetAllDevices();
            OutputData["Devices"] = serializationService.Serialize(devices);
            return true;
        }

        public bool UpdateDevice(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Device")) throw new ServerDefinedException("Нет поля Device");
            equipmentManager.SaveDevice(ParseDevice(InputData["Device"].ToString()));
            return true;
        }

        public bool DeleteDevice(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Device")) throw new ServerDefinedException("Нет поля Device");
            equipmentManager.DeleteDevice(ParseDevice(InputData["Device"].ToString()));
            return true;
        }


        private List<DeviceState> ParseDevicesStates(string JSON)
        {
            try
            {
                List<DeviceState> result = serializationService.Deserialize<List<DeviceState>>(JSON);
                if (result == null)
                    throw new ServerDefinedException("Неправильный формат входных данных статусов устройств");
                return result;
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Не удалось распарсить статусы устройств: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Не удалось распарсить список статусов устройств", ex);
                throw new ServerDefinedException("Неправильный формат входных данных статусов устройств");
            }
        }

        public bool GetLastDevicesStates(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<DeviceState> states = equipmentManager.GetLastDevicesStates();
            OutputData["DevicesStates"] = serializationService.Serialize(states);
            return true;
        }

        public bool AddDevicesStates(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("DevicesStates")) throw new ServerDefinedException("Нет поля DevicesStates");
            equipmentManager.AddDevicesStates(ParseDevicesStates(InputData["DevicesStates"].ToString()));
            return true;
        }


        private List<DeviceEvent> ParseDevicesEvents(string JSON)
        {
            try
            {
                List<DeviceEvent> result = serializationService.Deserialize<List<DeviceEvent>>(JSON);
                if (result == null)
                    throw new ServerDefinedException("Неправильный формат входных данных событий устройств");
                return result;
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Не удалось распарсить события устройств: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Не удалось распарсить список событий устройств", ex);
                throw new ServerDefinedException("Неправильный формат входных данных событий устройств");
            }
        }

        public bool GetDevicesEvents(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<DeviceEvent> events = equipmentManager.GetDevicesEvents(InputData);
            OutputData["DevicesEvents"] = serializationService.Serialize(events);
            return true;
        }

        public bool AddDevicesEvents(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("DevicesEvents")) throw new ServerDefinedException("Нет поля DevicesEvents");
            equipmentManager.AddDevicesEvents(ParseDevicesEvents(InputData["DevicesEvents"].ToString()));
            return true;
        }
    }
}
