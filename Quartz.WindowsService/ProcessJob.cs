using Quartz.Common;
using Quartz.WindowsService.Model;
using System.IO;

namespace Quartz.WindowsService
{
    /// <summary>
    /// Job da schedulare
    /// </summary>
    public class ProcessJob : IJob
    {
        /// <summary>
        /// Loggert
        /// </summary>
        private readonly BatchLogger Logger = BatchLogger.GetLogger();

        /// <summary>
        /// Esecuzione del Job
        /// </summary>
        /// <param name="context">Contesto di esecuzione</param>
        public void Execute(IJobExecutionContext context)
        {
            var dataMap = context.MergedJobDataMap;
            var scheduleConfiguration = (BatchScheduleConfiguration)dataMap["data"];

            if (!File.Exists(scheduleConfiguration.ProcessPath))
            {
                Logger.Error($"Non ho trovato nella cartella di del servizio Quartz l'eseguibile per {scheduleConfiguration.ProcessPath}", null);
                return;
            }

            Logger.Information($"Lancio {scheduleConfiguration.ProcessPath} {scheduleConfiguration.ProcessParameters}");
            Utilities.HeadelessProcessStart(scheduleConfiguration.ProcessPath, scheduleConfiguration.ProcessParameters);
        }
    }
}
