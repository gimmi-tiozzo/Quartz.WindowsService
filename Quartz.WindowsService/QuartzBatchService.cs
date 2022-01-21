using Polly;
using Quartz.Common;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.WindowsService.Core;
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
        /// <param name="oneShot">Indica se la configurazione delle schedulazioni deve essere recuperata con un solo accesso (oneShot) o un retry infinito</param>
        /// <returns>Configurazioni di schedulazione da database</returns>
        private List<BatchScheduleConfiguration> GetScheduleConfiguration(bool oneShot)
        {
            List<BatchScheduleConfiguration> schedules = new List<BatchScheduleConfiguration>();

            Policy.HandleResult<bool>(executionResult => executionResult == false)
                .WaitAndRetryForever(i => TimeSpan.FromMilliseconds(Convert.ToInt32(ConfigurationManager.AppSettings.Get(ConfigurationKeys.DbAccessRetrySleep))))
                .Execute(() =>
                {
                    try
                    {
                        BatchScheduleRepository repository = new BatchScheduleRepository();
                        schedules = repository.GetScheduleConfiguration(ConfigurationManager.AppSettings.Get(ConfigurationKeys.QuartzJobName), Environment.MachineName);
                        return true;
                    }
                    catch (Exception err)
                    {
                        Logger.Error(QuartzResources.DbError, err);
                        return oneShot;
                    }

                });

            return schedules;
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
                Logger.Information(String.Format(QuartzResources.NewSchedulerInfo, schedule));

                //definisci il Job (azione da eseguire)
                IJobDetail job = QuartzCore.CreateNewJob(schedule);
                //definisci il trigger (calendario) di tipo Cron (https://www.freeformatter.com/cron-expression-generator-quartz.html). es 0/5 * 8-17 * * ?
                ITrigger trigger = QuartzCore.CreateNewTrigger(schedule);

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
            //rischedula o nuova schedulazione job
            foreach (BatchScheduleConfiguration schedule in schedules)
            {
                //calcola la chiave del trigger associata al processo
                TriggerKey triggerKey = QuartzCore.GetTriggerKey(schedule.IdRule, schedule.BatchName);

                //cerca il trigger per capire se eseguire un update (rischedulazione) o una schedulazione (nuovo Job)
                ITrigger oldTrigger = Scheduler.GetTrigger(triggerKey);

                if (oldTrigger != null)
                {
                    IJobDetail newJob = QuartzCore.CreateNewJob(schedule);
                    ITrigger newTrigger = QuartzCore.CreateNewTrigger(schedule);

                    //update: delete & insert job + trigger
                    JobKey updateKey = QuartzCore.GetJobKeyByTriggerKey(triggerKey);
                    Scheduler.DeleteJob(updateKey);
                    Scheduler.ScheduleJob(newJob, newTrigger);
                    Logger.Information(String.Format(QuartzResources.UpdateJobInfo, updateKey.Name, updateKey.Group, schedule));
                }
                else
                {
                    IJobDetail newJob = QuartzCore.CreateNewJob(schedule);
                    ITrigger newTrigger = QuartzCore.CreateNewTrigger(schedule);

                    //New: insert Job (job & trigger)
                    Scheduler.ScheduleJob(newJob, newTrigger);
                    Logger.Information(String.Format(QuartzResources.InsertJobInfo, newJob.Key.Name, newJob.Key.Group, schedule));
                }
            }

            //eliminazione schedulazioni
            var allTriggerKeys = Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());

            foreach (TriggerKey triggerKey in allTriggerKeys)
            {
                if (!schedules.Any(s => QuartzCore.GetTriggerKey(s.IdRule, s.BatchName).Name == triggerKey.Name))
                {
                    //Cancel: delete Job (job)
                    JobKey deleteKey = QuartzCore.GetJobKeyByTriggerKey(triggerKey);
                    Scheduler.DeleteJob(deleteKey);
                    Logger.Information(String.Format(QuartzResources.CancelJobInfo, deleteKey.Name, deleteKey.Group));
                }
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

            //Policy di scandeza della configurazione scheduler caricata in cache
            CacheItemPolicy policy = new CacheItemPolicy();

            if (!Convert.ToBoolean(ConfigurationManager.AppSettings.Get(ConfigurationKeys.EnablePolicyAbsoluteExpiration))) 
            {
                policy.Priority = CacheItemPriority.NotRemovable;
                policy.AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
            }
            else
            {
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(Convert.ToInt32(ConfigurationManager.AppSettings.Get(ConfigurationKeys.CacheItemPolicyAbsoluteExpiration)));
                policy.RemovedCallback = new CacheEntryRemovedCallback(CacheRemovedCallback);
            }

            SchedulePlanCache.Set(ConfigurationKeys.SchedulePlanKey, schedules, policy);
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
                    Logger.Information(QuartzResources.PlainExpiredInfo);

                    try
                    {
                        //recupero piani di esecuzione da database
                        List<BatchScheduleConfiguration> schedules = GetScheduleConfiguration(true);
                        
                        if (schedules.Count > 0)
                        {
                            //ricarico nuovo piano di esecuzione in cache con nuova policy di scadenza
                            SetupSchedulePlanCache(schedules);
                            Logger.Information(QuartzResources.PlainReloadInfo);

                            //rischedulo il job con il nuovo piano di esecuzione recuparato da database
                            Reschedule(schedules);
                            Logger.Information(QuartzResources.RescheduleJobInfo);
                        }
                        else
                        {
                            //ricarico vecchio piano di esecuzione
                            SetupSchedulePlanCache((List<BatchScheduleConfiguration>)arguments.CacheItem.Value);
                            Logger.Error(String.Format(QuartzResources.PlainReloadNotFoundError, ConfigurationManager.AppSettings.Get(ConfigurationKeys.QuartzJobName), Environment.MachineName), null);
                        }
                    }
                    catch (Exception err)
                    {
                        Logger.Error(QuartzResources.PlainReloadError, err);

                        try
                        {
                            //ricarico vecchio piano di esecuzione
                            SetupSchedulePlanCache((List<BatchScheduleConfiguration>)arguments.CacheItem.Value);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(QuartzResources.PlainReloadError, ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Avvia il servizio
        /// </summary>
        private void StartService()
        {
            try
            {
                Logger.Information(QuartzResources.OnStartInfo);

                List<BatchScheduleConfiguration> schedules = GetScheduleConfiguration(false);

                if (schedules.Count > 0)
                {
                    //inizializza schedule e cache (piani di esecuzione)
                    InitializeScheduler();
                    SetupScheduler(schedules);
                    SetupSchedulePlanCache(schedules);

                    //avvia la schedulazione con tempificazine Cron
                    Scheduler.Start();
                }
                else
                {
                    Logger.Error(String.Format(QuartzResources.PlainLoadNotFoundError, ConfigurationManager.AppSettings.Get(ConfigurationKeys.QuartzJobName), Environment.MachineName), null);
                }
            }
            catch (Exception err)
            {
                Logger.Error(QuartzResources.OnStartError, err);
            }
        }

        /// <summary>
        /// Evento eccezioni non gestite
        /// </summary>
        /// <param name="sender">Origine evento</param>
        /// <param name="e">Parametri evento</param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(QuartzResources.GenericError, (Exception)e.ExceptionObject);
        }

        /// <summary>
        /// Metodo di start solo a fini di debug
        /// </summary>
        /// <param name="isStart">True si esegue start, false si esegue stop</param>
        public void OnDebug(bool isStart)
        {
            Logger.Information(QuartzResources.OnDebugInfo);

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
                //l'avvio dello scheduler è lanciato in un thread esterno in background nel caso di servizio per non bloccare OnStart della console dei servizi windows
                Thread trd = new Thread(new ThreadStart(() =>
                {
                    StartService();
                }))
                {
                    //se non sono in modalità user interactive (servizio windows) allora il thread è lanciato in background, altrimenti in foreground
                    IsBackground = !Environment.UserInteractive
                };

                trd.Start();
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
                    Logger.Information(QuartzResources.OnStopInfo);

                    if (Scheduler != null)
                    {
                        //spengo lo schedulatore
                        Scheduler.Shutdown();

                        //se è stata inizializzata una cache allora recupera eventuali processi da killare dai piani di esecuzione e pulisci la cache
                        if (SchedulePlanCache != null)
                        {
                            //ottengo l'eventuale piano di esecuzione
                            List<BatchScheduleConfiguration> scheduleConfigurations = (List<BatchScheduleConfiguration>)SchedulePlanCache.Get(ConfigurationKeys.SchedulePlanKey);

                            //pulisco la cache
                            SchedulePlanCache.ToList().ForEach(item => SchedulePlanCache.Remove(item.Key));

                            //killo eventuali processi in esecuzione
                            if (scheduleConfigurations != null && scheduleConfigurations.Count > 0)
                            {
                                //ottengo la lista in distinct di tutti i processi da killare dai piani di esecuzione
                                HashSet<string> processesToKill = new HashSet<string>();
                                scheduleConfigurations.ForEach(s => s.ProcessListToKill.ToList().ForEach(p => processesToKill.Add(p)));
                                string[] processesToKillArray = processesToKill.ToArray();

                                //eseguo kill dei processi
                                Logger.Information(String.Format(QuartzResources.OnStopProcessesKill, String.Join(", ", processesToKillArray)));
                                Utilities.KillProcessesByName(processesToKillArray);
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    Logger.Error(QuartzResources.OnStopError, err);
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
                    Logger.Information(QuartzResources.OnPauseInfo);

                    if (Scheduler != null)
                    {
                        Scheduler.PauseAll();
                    }
                }
                catch (Exception err)
                {
                    Logger.Error(QuartzResources.OnPauseError, err);
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
                    Logger.Information(QuartzResources.OnContinueInfo);

                    if (Scheduler != null)
                    {
                        Scheduler.ResumeAll();
                    }
                }
                catch (Exception err)
                {
                    Logger.Error(QuartzResources.OnContinueError, err);
                }
            }
        }
    }
}
