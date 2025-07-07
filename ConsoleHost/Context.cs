using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Core.Interfaces;

namespace Microsoft.ServiceModel.Samples
{
    public class Context : IContext
    {
        private object locker = new object();

        Dictionary<string, IService> services;

        public Context()
        {
            services = new Dictionary<string, IService>();
        }

        public void AddService(IService Service)
        {
            lock (locker)
            {
                if (services.ContainsKey(Service.GetName()))
                    services.Remove(Service.GetName());

                services[Service.GetName()] = Service;
                Console.WriteLine("added {0}", Service.GetName());
            }
        }

        public IService GetService(string ServiceName)
        {
            lock (locker)
            {
                if (!services.ContainsKey(ServiceName))
                    return null;
                else
                    return services[ServiceName];
            }
        }

        public T GetService<T>(string ServiceName)
        {
            lock (locker)
            {
                if (!services.ContainsKey(ServiceName))
                    return default(T);
                else

                    return (T)services[ServiceName];
            }
        }

        public void Create()
        {
            lock (locker)
            {
                foreach (IService s in services.Values)
                    s.OnCreate(this);
            }
        }

        public void Start()
        {
            lock (locker)
            {
                foreach (IService s in services.Values)
                    s.OnStart();
            }
        }

        public void Stop()
        {
            lock (locker)
            {
                foreach (IService s in services.Values)
                    s.OnStop();
            }
        }
    }
}
