using System;
using System.ComponentModel;
using System.ServiceProcess;

namespace LSOmni.WinService
{
    [RunInstaller(true)]
    public class CustomServiceInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;
        public static string InstallServiceName = "LSCommerceWinService";

        public CustomServiceInstaller()
        {
            try
            {
                process = new ServiceProcessInstaller();
                process.Account = ServiceAccount.LocalSystem;

                service = new ServiceInstaller();

                // 
                // ServiceProcessInstaller
                // 
                this.process.Account = ServiceAccount.LocalService;
                this.process.Password = null;
                this.process.Username = null;
                // 
                // ServiceInstaller
                // 
                this.service.Description = "Runs scheduled jobs for LS Commerce";
                this.service.DisplayName = "LS Commerce Windows Service";
                this.service.ServiceName = InstallServiceName;
                this.service.StartType = ServiceStartMode.Automatic;

                Installers.Add(process);
                Installers.Add(service);
            }
            catch (Exception)
            {
            }
        }
    }
}
