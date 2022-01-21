using Quartz.Common;
using Quartz.WindowsService.Model;

namespace Quartz.WindowsService.Core
{
    /// <summary>
    /// Gestione trigger e job
    /// </summary>
    public class QuartzCore
    {
        /// <summary>
        /// Prefisso per una chiave di un trigger
        /// </summary>
        private const string TriggerPrefix = "_TRIGGER";

        /// <summary>
        /// Prefisso per una chiave di un Job
        /// </summary>
        private const string JobPrefix = "_JOB";

        /// <summary>
        /// Crea un trigger
        /// </summary>
        /// <param name="idRule">Id regola</param>
        /// <param name="batchName">Nome batch</param>
        /// <returns>TriggerKey che rappresenta una chiave di un trigger</returns>
        public static TriggerKey GetTriggerKey(string idRule, string batchName)
        {
            return new TriggerKey($"{idRule}{TriggerPrefix}", $"{batchName}{TriggerPrefix}");
        }

        /// <summary>
        /// Crea un trigger
        /// </summary>
        /// <param name="idRule">Id regola</param>
        /// <param name="batchName">Nome batch</param>
        /// <returns>JobKey che rappresenta una chiave di un trigger</returns>
        public static JobKey GetJobKey(string idRule, string batchName)
        {
            return new JobKey($"{idRule}{JobPrefix}", $"{batchName}{JobPrefix}");
        }

        /// <summary>
        /// Ottieni la chiave di un Job dalla chiave di un trigger
        /// </summary>
        /// <param name="triggerKey">Chiave trigger</param>
        /// <returns>Chiave Job</returns>
        public static JobKey GetJobKeyByTriggerKey(TriggerKey triggerKey)
        {
            return new JobKey(Utilities.ReplaceLast(TriggerPrefix, JobPrefix, triggerKey.Name), Utilities.ReplaceLast(TriggerPrefix, JobPrefix, triggerKey.Group));
        }

        /// <summary>
        /// Crea un nuovo Job
        /// </summary>
        /// <param name="schedule">Regola di schedulazione</param>
        /// <returns>Nuovo Job</returns>
        public static IJobDetail CreateNewJob(BatchScheduleConfiguration schedule)
        {
            JobKey jobKey = GetJobKey(schedule.IdRule, schedule.BatchName);
            IJobDetail job = JobBuilder.Create<ProcessJob>().WithIdentity(jobKey.Name, jobKey.Group).Build();
            job.JobDataMap.Put(ProcessJob.JobDataMapName, schedule);

            return job;
        }

        /// <summary>
        /// Crea un nuovo Trigger
        /// </summary>
        /// <param name="schedule">Regola di schedulazione</param>
        /// <returns>Nuovo Trigger</returns>
        public static ITrigger CreateNewTrigger(BatchScheduleConfiguration schedule)
        {
            TriggerKey tupleKey = GetTriggerKey(schedule.IdRule, schedule.BatchName);
            ITrigger trigger = TriggerBuilder.Create().WithIdentity(tupleKey.Name, tupleKey.Group).WithCronSchedule(schedule.CronExpression).Build();

            return trigger;
        }
    }
}
