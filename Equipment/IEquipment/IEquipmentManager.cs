using System;
using System.Collections.Generic;

namespace Hermes.Parking.Server.Equipment
{
    public interface IEquipmentManager
    {
        Device GetDeviceById(int Id);
        IEnumerable<Device> GetAllDevices();
        void RegisterDevice(Device Device);
        void SaveDevice(Device Device);
        void DeleteDevice(Device Device);

        IEnumerable<DeviceState> GetLastDevicesStates();
        void AddDevicesStates(IEnumerable<DeviceState> States);

        IEnumerable<DeviceEvent> GetDevicesEvents(IDictionary<string, object> Filter);
        void AddDevicesEvents(IEnumerable<DeviceEvent> Events);
    }
}
