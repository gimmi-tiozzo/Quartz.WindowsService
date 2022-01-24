using Quartz.Common;
using Quartz.WindowsService.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quartz.WindowsService.Core
{
    /// <summary>
    /// Gestione trigger e job
    /// </summary>
    public class QuartzCore
    {
        /// <summary>
        /// Suffisso per una chiave di un trigger
        /// </summary>
        private const string TriggerSuffix = "_TRIGGER";

        /// <summary>
        /// Suffiso per una chiave di un Job
        /// </summary>
        private const string JobSuffix = "_JOB";

        /// <summary>
        /// Crea una chiave per un trigger
        /// </summary>
        /// <param name="name">Nome regola</param>
        /// <param name="group">Nome gruppo regola</param>
        /// <returns>TriggerKey che rappresenta una chiave di un trigger</returns>
        public static TriggerKey GetTriggerKey(string name, string group)
        {
            return new TriggerKey($"{name}{TriggerSuffix}", $"{group}{TriggerSuffix}");
        }

        /// <summary>
        /// Crea una chiave per il trigger primario (il primo)
        /// </summary>
        /// <param name="name">Nome regola</param>
        /// <param name="group">Nome gruppo regola</param>
        /// <returns>TriggerKey che rappresenta una chiave di un trigger</returns>
        public static TriggerKey GetFirstTriggerKey(string name, string group)
        {
            return new TriggerKey($"0_{name}{TriggerSuffix}", $"{group}{TriggerSuffix}");
        }

        /// <summary>
        /// Crea una chiave per un Job
        /// </summary>
        /// <param name="name">Nome regola</param>
        /// <param name="group">Nome gruppo regola</param>
        /// <returns>JobKey che rappresenta una chiave di un trigger</returns>
        public static JobKey GetJobKey(string name, string group)
        {
            return new JobKey($"0_{name}{JobSuffix}", $"{group}{JobSuffix}");
        }

        /// <summary>
        /// Ottieni la chiave di un Job dalla chiave di un trigger
        /// </summary>
        /// <param name="triggerKey">Chiave trigger</param>
        /// <returns>Chiave Job</returns>
        public static JobKey GetJobKeyByTriggerKey(TriggerKey triggerKey)
        {
            string triggerGroup = Utilities.ReplaceLast(TriggerSuffix, JobSuffix, triggerKey.Group);
            string triggerName = Utilities.ReplaceLast(TriggerSuffix, JobSuffix, triggerKey.Name);
            string jobName = $"0_{triggerName.Substring(2)}";

            return new JobKey(jobName, triggerGroup);
        }

        /// <summary>
        /// Crea un nuovo Job
        /// </summary>
        /// <param name="schedule">Regola di schedulazione</param>
        /// <returns>Nuovo Job</returns>
        public static IJobDetail CreateNewJob(BatchScheduleConfiguration schedule)
        {
            JobKey jobKey = GetJobKey(schedule.IdRule, schedule.IdRule);
            IJobDetail job = JobBuilder.Create<ProcessJob>().StoreDurably(true).WithIdentity(jobKey.Name, jobKey.Group).Build();
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
            TriggerKey triggerKey = GetTriggerKey(schedule.IdRule, schedule.IdRule);
            ITrigger trigger = TriggerBuilder.Create().WithIdentity(triggerKey.Name, triggerKey.Group).WithCronSchedule(schedule.CronExpression).Build();

            return trigger;
        }

        /// <summary>
        /// Crea dei Trigger associati ad un Job
        /// </summary>
        /// <param name="schedule">Regola di schedulazione</param>
        /// <param name="job">Job a cui associare i trigger ottenuti dalla regola di schedulazione</param>
        /// <returns>Nuovo Trigger</returns>
        public static List<ITrigger> CreateNewTriggersForJob(BatchScheduleConfiguration schedule, IJobDetail job)
        {
            //splitta le singole regole cron in AND (&)
            List<ITrigger> triggers = new List<ITrigger>();
            List<string> cronRules = schedule.CronExpression.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList();

            //per ogni regola cron crea un trigger e associalo al job
            for(int i = 0; i < cronRules.Count; i++)
            {
                //la chiave di un trigger è univoca: contatore_id regola a database. Il gruppo è dato solo da id regola a database
                TriggerKey triggerKey = GetTriggerKey($"{i}_{schedule.IdRule}", schedule.IdRule);
                ITrigger trigger = TriggerBuilder.Create().WithIdentity(triggerKey.Name, triggerKey.Group).WithCronSchedule(cronRules[i]).ForJob(job).Build();

                triggers.Add(trigger);
            }

            return triggers;
        }
    }
}
