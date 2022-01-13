using Quartz.Common;
using System;
using System.ServiceProcess;

namespace Quartz.WindowsService
{
    /// <summary>
    /// Entry point
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Loggert
        /// </summary>
        private static readonly BatchLogger Logger = BatchLogger.GetLogger();

        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        static void Main()
        {

            if (Environment.UserInteractive)
            {
                try
                {
                    Logger.Information("Avvio in modalità UserInteractive");
                    new QuartzBatchService().OnDebug();
                }
                catch (Exception err)
                {
                    Logger.Error("Errore Batch Service in modalità interattiva (OnDebug)", err);
                }
            }
            else
            {
                Logger.Information("Avvio in modalità Windows Service (Headless)");

                ServiceBase[] servicesToRun = new ServiceBase[] { new QuartzBatchService() };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
