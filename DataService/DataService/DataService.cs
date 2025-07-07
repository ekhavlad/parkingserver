using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Core.Interfaces;

namespace Hermes.Parking.Server.DataService
{
    public class DataService : BaseService, IDataService
    {
        private IDictionary<string, IList<IDataProvider>> objectTypes = new Dictionary<string, IList<IDataProvider>>();
        public IDictionary<string, IList<IDataProvider>> ObjectTypes { get { return objectTypes; } }

        public void AddDataProvider(string ObjectType, IDataProvider Provider)
        {
            Logger.DebugFormat("ObjectType = '{0}', Provider = '{1}'", ObjectType, Provider.GetName());

            if (!ObjectTypes.ContainsKey(ObjectType))
            {
                ObjectTypes.Add(ObjectType, new List<IDataProvider>());
                Logger.InfoFormat("ObjectType '{0}' has been registered", ObjectType);
            }

            if (ObjectTypes[ObjectType].Contains(Provider))
            {
                ObjectTypes[ObjectType].Remove(Provider);
                ObjectTypes[ObjectType].Add(Provider);
                Logger.WarnFormat("Data provider '{0}' has been re-subscribed to the ObjectType '{1}'", Provider.GetName(), ObjectType);
            }
            else
            {
                ObjectTypes[ObjectType].Add(Provider);
                Logger.InfoFormat("Data provider '{0}' has been subscribed to the ObjectType '{1}'", Provider.GetName(), ObjectType);
            }
        }

        public void RemoveDataProvider(string ObjectType, IDataProvider Provider)
        {
            Logger.DebugFormat("ObjectType = '{0}', Data provider = '{1}'", ObjectType, Provider.GetName());

            if (!ObjectTypes.ContainsKey(ObjectType))
            {
                Logger.WarnFormat("ObjectType '{0}' has not been registered", ObjectType);
            }
            else if (ObjectTypes[ObjectType].Contains(Provider))
            {
                ObjectTypes[ObjectType].Remove(Provider);
                Logger.InfoFormat("Data provider '{0}' has been unsubscribed from the object type '{1}'", Provider.GetName(), ObjectType);

                if (ObjectTypes[ObjectType].Count == 0)
                {
                    ObjectTypes.Remove(ObjectType);
                    Logger.InfoFormat("ObjectType '{0}' has been removed because of no data providers left", ObjectType);
                }
            }
            else
            {
                Logger.WarnFormat("Data provider '{0}' has not been subscribed to the object type '{1}'", Provider.GetName(), ObjectType);
            }
        }

        public override string GetName() { return Constants.DATA_SERVICE_NAME; }

        public T Create<T>(string ObjectType, object InitialObject)
        {
            object output = null;

            foreach (IDataProvider provider in ObjectTypes[ObjectType])
                provider.Create(ObjectType, InitialObject, ref output);

            return (T)output;
        }

        public T Get<T>(string ObjectType, IDictionary<string, object> Filter)
        {
            object output = null;

            foreach (IDataProvider provider in ObjectTypes[ObjectType])
                provider.Get(ObjectType, Filter, ref output);

            return (T)output;
        }

        public T Get<T>(string ObjectType, long Id)
        {
            object output = null;

            foreach (IDataProvider provider in ObjectTypes[ObjectType])
                provider.Get(ObjectType, Id, ref output);

            return (T)output;
        }

        public IEnumerable<T> GetList<T>(string ObjectType, IDictionary<string, object> Filter)
        {
            IEnumerable<object> output = null;

            foreach (IDataProvider provider in ObjectTypes[ObjectType])
                provider.GetList(ObjectType, Filter, ref output);

            return (IEnumerable<T>)output;
        }

        public void Save(string ObjectType, object Object)
        {
            foreach (IDataProvider provider in ObjectTypes[ObjectType])
                provider.Save(ObjectType, Object);
        }

        public void Delete(string ObjectType, object Object)
        {
            foreach (IDataProvider provider in ObjectTypes[ObjectType])
                provider.Delete(ObjectType, Object);
        }
    }
}
