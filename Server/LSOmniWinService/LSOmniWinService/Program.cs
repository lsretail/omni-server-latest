using System;
using System.ServiceProcess;

namespace LSOmni.WinService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var servicesToRun = new LSOmniService();
            if (Environment.UserInteractive)
            {
                if (args.Length > 0)
                {
                    for (int ii = 0; ii < args.Length; ii++)
                    {
                        switch (args[ii].ToUpper())
                        {
                            case "/i":
                            case "/I":
                                LSOmniService.InstallService();
                                return;
                            case "/u":
                            case "/U":
                                LSOmniService.UninstallService();
                                return;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    //good for debugging from vstudio
                    servicesToRun.Start();
                    Console.WriteLine("... press <ENTER> to quit");
                    Console.ReadLine();
                    servicesToRun.Stop();
                }
            }
            else
            {
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
