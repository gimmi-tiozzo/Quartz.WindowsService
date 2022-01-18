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
            try
            {
                if (Environment.UserInteractive)
                {
                    try
                    {
                        Logger.Information(QuartzResources.UserInteractiveInfo);
                        QuartzBatchService service = new QuartzBatchService();
                        service.OnDebug(isStart: true);
                    }
                    catch (Exception err)
                    {
                        Logger.Error(QuartzResources.OnMainDebugError, err);
                    }
                }
                else
                {
                    Logger.Information(QuartzResources.HeadlessInfo);

                    //imposta la directory corrente a quella dell'eseguibile altrimenti per un windows service sarebbe C:\WINDOWS\system32
                    Logger.Information(String.Format(QuartzResources.PreCurrentDirectoryInfo, Directory.GetCurrentDirectory()));
                    Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                    Logger.Information(String.Format(QuartzResources.PostCurrentDirectoryInfo, Directory.GetCurrentDirectory()));

                    ServiceBase[] servicesToRun = new ServiceBase[] { new QuartzBatchService() };
                    ServiceBase.Run(servicesToRun);
                }
            }
            catch (Exception err)
            {
                Logger.Error(QuartzResources.GenericError, err);
            }
        }
    }
}
