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
        public static string InstallServiceName = "LSOmniWinService";

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
                this.service.Description = "Runs scheduled jobs for LS Omni";
                this.service.DisplayName = "LS Omni Win Service";
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
