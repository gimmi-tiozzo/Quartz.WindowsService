using Quartz.Common;
using System;
using System.IO;
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
                    QuartzBatchService service = new QuartzBatchService();
                    service.OnDebug(isStart: true);
                }
                catch (Exception err)
                {
                    Logger.Error("Errore Batch Service in modalità interattiva (OnDebug)", err);
                }
            }
            else
            {
                Logger.Information("Avvio in modalità Windows Service (Headless)");

                //imposta la directory corrente a quella dell'eseguibile altrimenti per un windows service sarebbe C:\WINDOWS\system32
                Logger.Information($"Current Working Directory prima di set forzato: {Directory.GetCurrentDirectory()}");
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                Logger.Information($"Current Working Directory dopo set forzato: {Directory.GetCurrentDirectory()}");

                ServiceBase[] servicesToRun = new ServiceBase[] { new QuartzBatchService() };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
