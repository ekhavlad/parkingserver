using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ServiceModel;
using System.ServiceProcess;
using System.Configuration;

using System.Configuration.Install;
namespace Microsoft.ServiceModel.Samples
{
    [RunInstaller(true)]
    public class ParkingServerServiceInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public ParkingServerServiceInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "ParkingServerService";
            Installers.Add(process);
            Installers.Add(service);
        }
    }
}
