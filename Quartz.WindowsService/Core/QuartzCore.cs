using Quartz.WindowsService.Model;
using System;

namespace Quartz.WindowsService.Core
{
    /// <summary>
    /// Gestione trigger e job
    /// </summary>
    public class QuartzCore
    {
        /// <summary>
        /// Crea un trigger
        /// </summary>
        /// <param name="idRule">Id regola</param>
        /// <param name="batchName">Nome batch</param>
        /// <returns>Tupla che rappresenta una chiave di un trigger</returns>
        public static Tuple<string, string> GetTriggerKey(string idRule, string batchName)
        {
            return new Tuple<string, string>($"{idRule}_TRIGGER", $"{batchName}_TRIGGER");
        }

        /// <summary>
        /// Crea un trigger
        /// </summary>
        /// <param name="idRule">Id regola</param>
        /// <param name="batchName">Nome batch</param>
        /// <returns>Tupla che rappresenta una chiave di un trigger</returns>
        public static Tuple<string, string> GetJobKey(string idRule, string batchName)
        {
            return new Tuple<string, string>($"{idRule}_JOB", $"{batchName}_JOB");
        }

        /// <summary>
        /// Crea un nuovo Job
        /// </summary>
        /// <param name="schedule">Regola di schedulazione</param>
        /// <returns>Nuovo Job</returns>
        public static IJobDetail CreateNewJob(BatchScheduleConfiguration schedule)
        {
            var jobKey = GetJobKey(schedule.IdRule, schedule.BatchName);
            IJobDetail job = JobBuilder.Create<ProcessJob>().WithIdentity(jobKey.Item1, jobKey.Item2).Build();
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
            var tupleKey = GetTriggerKey(schedule.IdRule, schedule.BatchName);
            ITrigger trigger = TriggerBuilder.Create().WithIdentity(tupleKey.Item1, tupleKey.Item2).WithCronSchedule(schedule.CronExpression).Build();

            return trigger;
        }
    }
}
