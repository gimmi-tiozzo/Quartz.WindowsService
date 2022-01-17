using Quartz.Common;
using Quartz.Impl;
using Quartz.WindowsService.Database;
using Quartz.WindowsService.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceProcess;
using System.Threading;

namespace Quartz.WindowsService
{
    /// <summary>
    /// Batch (Servizio Windows) che lancia un eseguibile in modalità headeless in base ad una schedulazione di tipo Cron
    /// </summary>
    public partial class QuartzBatchService : ServiceBase
    {
        /// <summary>
        /// Scheduler
        /// </summary>
        protected IScheduler Scheduler { get; set; }

        /// <summary>
        /// Cache dei piani di schedulazione
        /// </summary>
        protected ObjectCache SchedulePlanCache { get; set; }

        /// <summary>
        /// Oggetto usato come mutex per la sincronizzazione tra thread diversi
        /// </summary>
        private readonly object Mutex = new object();

        /// <summary>
        /// Logger
        /// </summary>
        private readonly BatchLogger Logger = BatchLogger.GetLogger();

        /// <summary>
        /// Costruttore
        /// </summary>
        public QuartzBatchService()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Inizializza lo scheduler
        /// </summary>
        private void InitializeScheduler()
        {
            //costruisci la factory di default
            StdSchedulerFactory factory = new StdSchedulerFactory();

            //ottieni uno scheduler (chi lancia il job in base al trigger)
            Scheduler = factory.GetScheduler();
        }

        /// <summary>
        /// Ottieni le configurazioni di schedulazione da database
        /// </summary>
        /// <param name="oneShot">Indica se la configurazione delle schedulazioni deve essere recuperata con un solo accesso (oneShot) e un retry infinito</param>
        /// <returns>Configurazioni di schedulazione da database</returns>
        private List<BatchScheduleConfiguration> GetScheduleConfiguration(bool oneShot)
        {
            while (true)
            {
                try
                {
                    BatchScheduleRepository repository = new BatchScheduleRepository();
                    return repository.GetScheduleConfiguration(ConfigurationManager.AppSettings.Get("QuartzJobName"), Environment.MachineName);
                }
                catch (Exception err)
                {
                    if (!oneShot)
                    {
                        Logger.Error("Errore recupero configurazioni da database", err);
                    }
                    else
                    {
                        throw err;
                    }
                }

                //pausa ogni 15 secondi di retry
                Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings.Get("DbAccessRetrySleep")));
            }
        }

        /// <summary>
        /// Configura lo scheduler
        /// </summary>
        private void SetupScheduler(List<BatchScheduleConfiguration> schedules)
        {
            //dizionario dei job con relativi trigger di attivazione
            var jobsAndTriggers = new Dictionary<IJobDetail, Quartz.Collection.ISet<ITrigger>>();

            foreach (BatchScheduleConfiguration schedule in schedules)
            {
                Logger.Information($"Creazione Job e Trigger con configurazione: {schedule}");

                //definisci il Job (azione da eseguire)
                var jobKey = Utilities.GetJobKey(schedule.IdRule, schedule.BatchName);
                IJobDetail job = JobBuilder.Create<ProcessJob>().WithIdentity(jobKey.Item1, jobKey.Item2).Build();
                job.JobDataMap.Put(ProcessJob.JobDataMapName, schedule);

                //definisci il trigger (calendario) di tipo Cron (https://www.freeformatter.com/cron-expression-generator-quartz.html). es 0/5 * 8-17 * * ?
                var tupleKey = Utilities.GetTriggerKey(schedule.IdRule, schedule.BatchName);
                ITrigger trigger = TriggerBuilder.Create().WithIdentity(tupleKey.Item1, tupleKey.Item2).WithCronSchedule(schedule.CronExpression).Build();

                jobsAndTriggers.Add(job, new Quartz.Collection.HashSet<ITrigger>() { trigger });
            }

            //configura lo schedulatore
            Scheduler.ScheduleJobs(jobsAndTriggers, true);
        }

        /// <summary>
        /// Richedula i piani di esecuzione dello scheduler Quartz
        /// </summary>
        /// <param name="schedules">Piani di esecuzione Cron</param>
        private void Reschedule(List<BatchScheduleConfiguration> schedules)
        {
            foreach (BatchScheduleConfiguration schedule in schedules)
            {
                var tupleKey = Utilities.GetTriggerKey(schedule.IdRule, schedule.BatchName);
                TriggerKey triggerKey = new TriggerKey(tupleKey.Item1, tupleKey.Item2);

                ITrigger oldTrigger = Scheduler.GetTrigger(triggerKey);
                TriggerBuilder oldTriggerBuilder = oldTrigger.GetTriggerBuilder();
                ITrigger newTrigger = oldTriggerBuilder.WithCronSchedule(schedule.CronExpression).Build();

                Scheduler.RescheduleJob(triggerKey, newTrigger);
            }
        }

        /// <summary>
        /// Configura la cache di esecuzione dei piani dello scheduler
        /// </summary>
        /// <param name="schedules">Piani di esecuzione cron</param>
        private void SetupSchedulePlanCache(List<BatchScheduleConfiguration> schedules)
        {
            if (SchedulePlanCache == null)
            {
                SchedulePlanCache = MemoryCache.Default;
            }

            CacheItemPolicy policy = new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(Convert.ToInt32(ConfigurationManager.AppSettings.Get("CacheItemPolicyAbsoluteExpiration"))),
                RemovedCallback = new CacheEntryRemovedCallback(CacheRemovedCallback)
            };

            schedules.ForEach(s => SchedulePlanCache.Set(s.IdRule, s, policy));
        }

        /// <summary>
        /// Callback invocata alla scadenza dei piani di esecuzione in cache
        /// </summary>
        /// <param name="arguments">Argomento callback</param>
        private void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            lock (Mutex)
            {
                if (!Scheduler.IsShutdown)
                {
                    Logger.Information("Piani di esecuzione in cache scaduti");

                    try
                    {
                        List<BatchScheduleConfiguration> schedules = GetScheduleConfiguration(true);
                        
                        if (schedules.Count > 0)
                        {
                            SetupSchedulePlanCache(schedules);
                            Logger.Information("Ricaricarti nuovi piani di esecuzione in cache");

                            Reschedule(schedules);
                            Logger.Information("Richedulati Job quartz con nuovi piani di esecuzione Cron");
                        }
                        else
                        {
                            Logger.Warning($"Non ho trovato piani di esecuzione a database per il batch {ConfigurationManager.AppSettings.Get("QuartzJobName")} e server {Environment.MachineName}. Si mantengono quelli attuali");
                        }
                    }
                    catch (Exception err)
                    {
                        Logger.Error("Piani di esecuzion in cache scaduti error. Si mantengono quelli attuali", err);
                    }
                }
            }
        }

        /// <summary>
        /// Evento eccezioni non gestite
        /// </summary>
        /// <param name="sender">Origine evento</param>
        /// <param name="e">Parametri evento</param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error("Errore non gestito", (Exception)e.ExceptionObject);
        }

        /// <summary>
        /// Metodo di start solo a fini di debug
        /// </summary>
        /// <param name="isStart">True si esegue start, false si esegue stop</param>
        public void OnDebug(bool isStart)
        {
            Logger.Information("Esecuzione OnDebug");

            if (isStart)
            {
                OnStart(null);
            }
            else
            {
                OnStop();
            }
        }

        /// <summary>
        /// Avvio servizio
        /// </summary>
        /// <param name="args">Argomenti passati da console servizi</param>
        protected override void OnStart(string[] args)
        {
            lock (Mutex)
            {
                try
                {
                    Logger.Information("Esecuzione OnStart");

                    List<BatchScheduleConfiguration> schedules = GetScheduleConfiguration(false);

                    if (schedules.Count > 0)
                    {
                        InitializeScheduler();
                        SetupScheduler(schedules);
                        SetupSchedulePlanCache(schedules);

                        Scheduler.Start();
                    }
                    else
                    {
                        Logger.Warning($"Non ho trovato piani di esecuzione a database per il batch {ConfigurationManager.AppSettings.Get("QuartzJobName")} e server {Environment.MachineName}. Non attivo lo scheduler quartz e la cache dei piani di esecuzione");
                    }
                }
                catch (Exception err)
                {
                    Logger.Error("OnStart error", err);
                }

            }
            
        }

        /// <summary>
        /// Stop Servizio
        /// </summary>
        protected override void OnStop()
        {
            lock (Mutex)
            {
                try
                {
                    Logger.Information("Esecuzione OnStop");

                    if (Scheduler != null)
                    {
                        //spengo lo schedulatore
                        Scheduler.Shutdown();

                        //eseguo kill dei processi
                        foreach (var item in SchedulePlanCache)
                        {
                            BatchScheduleConfiguration scheduleConfiguration = (BatchScheduleConfiguration)item.Value;

                            Logger.Information($"Provo ad eseguire il kill dei processi: {String.Join(", ", scheduleConfiguration.ProcessListToKill)}");
                            Utilities.KillProcessesByName(scheduleConfiguration.ProcessListToKill);
                        }

                        //pulisco la cache
                        SchedulePlanCache.ToList().ForEach(item => SchedulePlanCache.Remove(item.Key));
                    }
                }
                catch (Exception err)
                {
                    Logger.Error("OnStop error", err);
                }
            }
            
        }

        /// <summary>
        /// Pausa servizio
        /// </summary>
        protected override void OnPause()
        {
            lock (Mutex)
            {
                try
                {
                    Logger.Information("Esecuzione OnPause");

                    if (Scheduler != null)
                    {
                        Scheduler.PauseAll();
                    }
                }
                catch (Exception err)
                {
                    Logger.Error("OnPause error", err);
                }
            }
        }

        /// <summary>
        /// Continua servizio precedentemente fermato in pausa
        /// </summary>
        protected override void OnContinue()
        {
            lock (Mutex)
            {
                try
                {
                    Logger.Information("Esecuzione OnContinue");

                    if (Scheduler != null)
                    {
                        Scheduler.ResumeAll();
                    }
                }
                catch (Exception err)
                {
                    Logger.Error("OnContinue error", err);
                }
            }
        }
    }
}
