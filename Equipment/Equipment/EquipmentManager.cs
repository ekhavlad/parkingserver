using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Parking.Server;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using Hermes.Parking.Server.FinanceService;
using System.Data;
using Hermes.Parking.Server.EventService;
using Hermes.Parking.Server.RequestProcessor;

namespace Hermes.Parking.Server.Equipment
{
    public class EquipmentManager : BaseService, IEquipmentManager
    {
        private IDataService dataService;
        private ISecurityManager securityManager;
        private IFinanceService financeService;
        private IEventService eventService;

        private object locker = new object();
        private Dictionary<int, Device> devices;

        public override string GetName() { return Constants.EQUIPMENT_MANAGER_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            this.dataService = Context.GetService<IDataService>(DataService.Constants.DATA_SERVICE_NAME);
            this.securityManager = Context.GetService<ISecurityManager>(RequestProcessor.Constants.SECURITY_MANAGER_NAME);
            this.financeService = Context.GetService<IFinanceService>(FinanceService.Constants.FINANCE_SERVICE_NAME);
            this.eventService = Context.GetService<IEventService>(Hermes.Parking.Server.EventService.Constants.EVENT_SERVICE_NAME);
        }

        public override void OnStart()
        {
            lock (locker)
            {
                devices = dataService.GetList<Device>(Constants.DEVICE_OBJECT_TYPE_NAME, null).ToDictionary(x => x.Id, x => x);
            }
        }


        public Device GetDeviceById(int Id)
        {
            lock (locker)
            {
                if (devices.ContainsKey(Id))
                    return (Device)devices[Id].Clone();
                else
                    return null;
            }
        }

        public IEnumerable<Device> GetAllDevices()
        {
            lock (locker)
            {
                return devices.Select(x => (Device)x.Value.Clone()).ToList();
            }
        }

        public void RegisterDevice(Device Device)
        {
            try
            {
                lock (locker)
                {
                    if (Device.ParentId.HasValue && GetDeviceById(Device.ParentId.Value) == null)
                        throw new ServerDefinedException(string.Format("Для устройства {0} ({1}) указано несуществующее родительское устройство {2}", Device.Id, Device.Name, Device.ParentId.Value));

                    Logger.DebugFormat("Регистрируем устройство {0}", Device);
                    Device tmp = dataService.Create<Device>(Constants.DEVICE_OBJECT_TYPE_NAME, Device);
                    Device.Id = tmp.Id;
                    devices[Device.Id] = (Device)Device.Clone();
                    Logger.InfoFormat("Устройство {0} зарегистрировано", Device);

                    Dictionary<string, object> eventData = new Dictionary<string, object>();
                    eventData["Device"] = (Device)Device.Clone();
                    eventService.EvokeEvent(Constants.DEVICE_CREATED_EVENT_NAME, eventData);
                }
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при регистрации устройства: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при регистрации устройства", ex2);
                throw new Exception("Ошибка при регистрации устройства");
            }
        }

        public void SaveDevice(Device Device)
        {
            try
            {
                lock (locker)
                {

                    if (!devices.ContainsKey(Device.Id))
                        throw new ServerDefinedException(string.Format("Устройство {0} ({1}) не зарегистрировано в системе", Device.Id, Device.Name));

                    if (Device.ParentId.HasValue && GetDeviceById(Device.ParentId.Value) == null)
                        throw new ServerDefinedException(string.Format("Для устройства {0} ({1}) указано несуществующее родительское устройство {2}", Device.Id, Device.Name, Device.ParentId.Value));

                    Device original = (Device)devices[Device.Id].Clone();

                    Logger.DebugFormat("Обновляем устройство {0}", Device);
                    dataService.Save(Constants.DEVICE_OBJECT_TYPE_NAME, Device);
                    devices[Device.Id] = (Device)Device.Clone();
                    Logger.InfoFormat("Устройство {0} обновлено", Device);

                    Dictionary<string, object> eventData = new Dictionary<string, object>();
                    eventData["Device"] = (Device)Device.Clone();
                    eventData["OriginalDevice"] = original;
                    eventService.EvokeEvent(Constants.DEVICE_CHANGED_EVENT_NAME, eventData);
                }
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при обновлении устройства: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при обновлении устройства", ex2);
                throw new Exception("Ошибка при обновлении устройства");
            }
        }

        public void DeleteDevice(Device Device)
        {
            try
            {
                lock (locker)
                {
                    if (!devices.ContainsKey(Device.Id))
                        throw new ServerDefinedException(string.Format("Устройство с Id = {0} не зарегистрировано в системе", Device.Id));
                    
                    Logger.DebugFormat("Удаляем устройство с Id = {0}", Device.Id);
                    dataService.Delete(Constants.DEVICE_OBJECT_TYPE_NAME, Device.Id);
                    devices.Remove(Device.Id);
                    Logger.InfoFormat("Устройство с Id = {0} удалено", Device.Id);

                    Dictionary<string, object> eventData = new Dictionary<string, object>();
                    eventData["DeviceId"] = Device.Id;
                    eventService.EvokeEvent(Constants.DEVICE_DELETED_EVENT_NAME, eventData);
                }
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при удалении устройства: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при удалении устройства", ex2);
                throw new Exception("Ошибка при удалении устройства");
            }
        }


        public IEnumerable<DeviceState> GetLastDevicesStates()
        {
            Dictionary<string, object> filter = new Dictionary<string, object>();
            filter["IsLast"] = true;
            IEnumerable<DeviceState> result = dataService.GetList<DeviceState>(Constants.DEVICE_STATE_OBJECT_TYPE_NAME, filter);
            return result;
        }

        public void AddDevicesStates(IEnumerable<DeviceState> States)
        {
            try
            {
                List<DeviceState> newStates = new List<DeviceState>();
                List<int> invalidDevices = new List<int>();
                foreach (DeviceState s in States)
                {
                    if (devices.ContainsKey(s.DeviceId))
                    {
                        Logger.DebugFormat("Регистрируем состояние {0}", s);
                        DeviceState newDeviceState = dataService.Create<DeviceState>(Constants.DEVICE_STATE_OBJECT_TYPE_NAME, s);
                        newStates.Add(newDeviceState);
                        Logger.InfoFormat("Состояние {0} зарегистрировано", s);
                    }
                    else
                    {
                        Logger.WarnFormat("Попытка сохранить состояние для незарегистрированного устройства {0}", s.DeviceId);
                        invalidDevices.Add(s.DeviceId);
                    }
                }

                if (invalidDevices.Count > 0)
                    eventService.EvokeEvent(SystemEventLevel.Warning, string.Format("Попытка сохранить состояния незарегистрированных устройств: {0}", string.Join(",", invalidDevices)));

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["DevicesStates"] = newStates;
                eventService.EvokeEvent(Constants.DEVICES_STATES_CREATED_EVENT_NAME, eventData);
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при создании статуса устройства: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при создании статуса устройства", ex2);
                throw new Exception("Ошибка при создании статуса устройства");
            }
        }


        public IEnumerable<DeviceEvent> GetDevicesEvents(IDictionary<string, object> Filter)
        {
            IEnumerable<DeviceEvent> result = dataService.GetList<DeviceEvent>(Constants.DEVICE_EVENT_OBJECT_TYPE_NAME, Filter);
            return result;
        }

        public void AddDevicesEvents(IEnumerable<DeviceEvent> Events)
        {
            try
            {
                List<DeviceEvent> newEvents = new List<DeviceEvent>();
                List<int> invalidDevices = new List<int>();
                foreach (DeviceEvent e in Events)
                {
                    if (devices.ContainsKey(e.DeviceId))
                    {
                        Logger.DebugFormat("Регистрируем событие {0}", e);
                        DeviceEvent newDeviceEvent = dataService.Create<DeviceEvent>(Constants.DEVICE_EVENT_OBJECT_TYPE_NAME, e);
                        newEvents.Add(newDeviceEvent);
                        Logger.InfoFormat("Событие {0} зарегистрировано", e);
                    }
                    else
                    {
                        Logger.WarnFormat("Попытка сохранить событие для незарегистрированного устройства {0}", e.DeviceId);
                        invalidDevices.Add(e.DeviceId);
                    }
                }

                if (invalidDevices.Count > 0)
                    eventService.EvokeEvent(SystemEventLevel.Warning, string.Format("Попытка сохранить события незарегистрированных устройств: {0}", string.Join(",", invalidDevices)));

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["DevicesEvents"] = newEvents;
                eventService.EvokeEvent(Constants.DEVICES_EVENTS_CREATED_EVENT_NAME, eventData);
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при создании события устройства: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при создании события устройства", ex2);
                throw new Exception("Ошибка при создании события устройства");
            }
        }
    }
}
