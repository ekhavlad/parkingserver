using System;
using Hermes.Core.Interfaces;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Activation;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace Hermes.Parking.Server.ServerAPI
{
    public class ServerAPIHost : BaseService
    {
        private object locker = new object();
        private ServiceHost host;

        public override string GetName() { return Constants.SERVER_API_HOST_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            // используем статический метод для инициализации,
            // т.к. нельзя передавать экземпляр хоста без конструктора по-умолчанию
            ServerAPI.Init(Context);
            host = new ServiceHost(typeof(ServerAPI));
        }

        public override void OnStart()
        {
            Logger.Debug("Opening host...");
            host.Open();
            Logger.Info("Server API host has just been opened");
        }

        public override void OnStop()
        {
            Logger.Debug("Closing the host...");
            ServerAPI.Stop();
            host.Close();
            Logger.Info("The host has just been closed");
        }
    }
}
