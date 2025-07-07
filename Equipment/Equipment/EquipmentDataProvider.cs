using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using System.Threading;

namespace Hermes.Parking.Server.Equipment
{
    public partial class EquipmentDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService db;

        public EquipmentDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
        }

        public override string GetName()
        {
            return Constants.EQUIPMENT_DATA_PROVIDER_NAME;
        }

        public override void Create(string ObjectType, object InitialObject, ref object Output)
        {
            switch (ObjectType)
            {
                case Constants.DEVICE_OBJECT_TYPE_NAME:
                    Output = CreateDevice((Device)InitialObject);
                    break;

                case Constants.DEVICE_STATE_OBJECT_TYPE_NAME:
                    Output = CreateDeviceState((DeviceState)InitialObject);
                    break;

                case Constants.DEVICE_EVENT_OBJECT_TYPE_NAME:
                    Output = CreateDeviceEvent((DeviceEvent)InitialObject);
                    break;
            }
        }

        public override void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
            switch (ObjectType)
            {
                case Constants.DEVICE_OBJECT_TYPE_NAME:
                    Output = GetDevices(Filter);
                    break;

                case Constants.DEVICE_STATE_OBJECT_TYPE_NAME:
                    Output = GetDevicesStates(Filter);
                    break;

                case Constants.DEVICE_EVENT_OBJECT_TYPE_NAME:
                    Output = GetDevicesEvents(Filter);
                    break;
            }
        }

        public override void Save(string ObjectType, object Object)
        {
            switch (ObjectType)
            {
                case Constants.DEVICE_OBJECT_TYPE_NAME:
                    SaveDevice((Device)Object);
                    break;
            }
        }

        public override void Delete(string ObjectType, object Object)
        {
            switch (ObjectType)
            {
                case Constants.DEVICE_OBJECT_TYPE_NAME:
                    DeleteDevice((int)Object);
                    break;
            }
        }
    }
}
