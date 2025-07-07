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
        
        public override string GetName() { return Constants.DEFAULT_SERVER_API_HOST_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            //ServerAPI api = new ServerAPI(Context);
            //host = new ServiceHost(api);
            host = new MyServiceHost(Context, typeof(ServerAPI));
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
            host.Close();
            Logger.Info("THe host has just been closed");
        }
    }


    public class MyServiceHostFactory : ServiceHostFactory
    {
        private readonly IContext context;

        public MyServiceHostFactory(IContext Context)
        {
            this.context = Context;
        }

        protected override ServiceHost CreateServiceHost(Type serviceType,
            Uri[] baseAddresses)
        {
            return new MyServiceHost(this.context, serviceType);
        }
    }

    public class MyServiceHost : ServiceHost
    {
        public MyServiceHost(IContext Context, Type serviceType)
            : base(serviceType)
        {

            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new MyInstanceProvider(Context));
            }
        }
    }

    public class MyInstanceProvider : IInstanceProvider, IContractBehavior
    {
        private readonly IContext context;

        public MyInstanceProvider(IContext Context)
        {
            this.context = Context;
        }


        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return new ServerAPI(this.context);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }



        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
    }
}
