using Quartz.Common;
using Quartz.Impl;
using Quartz.WindowsService.Database;
using Quartz.WindowsService.Model;
using System;
using System.Collections.Generic;
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
        /// <returns>Configurazioni di schedulazione da database</returns>
        private List<BatchScheduleConfiguration> GetScheduleConfiguration()
        {
            while (true)
            {
                try
                {
                    BatchScheduleRepository repository = new BatchScheduleRepository();
                    return repository.GetScheduleConfiguration("QUARTZ_TEST", Environment.MachineName);
                }
                catch (Exception err)
                {
                    Logger.Error("Errore recupero configurazioni da database", err);
                }

                //pausa ogni 15 secondi di retry
                Thread.Sleep(15000);
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
                IJobDetail job = JobBuilder.Create<ProcessJob>().WithIdentity($"{schedule.IdRule}_JOB", $"{schedule.BatchName}_JOB").Build();
                job.JobDataMap.Put("data", schedule);

                //definisci il trigger (calendario) di tipo Cron (https://www.freeformatter.com/cron-expression-generator-quartz.html). es 0/5 * 8-17 * * ?
                ITrigger trigger = TriggerBuilder.Create().WithIdentity($"{schedule.IdRule}_TRIGGER", $"{schedule.BatchName}_TRIGGER").WithCronSchedule(schedule.CronExpression).Build();

                jobsAndTriggers.Add(job, new Quartz.Collection.HashSet<ITrigger>() { trigger });
            }

            //configura lo schedulatore
            Scheduler.ScheduleJobs(jobsAndTriggers, true);
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
        public void OnDebug()
        {
            Logger.Information("Esecuzione OnDebug");
            OnStart(null);
        }

        /// <summary>
        /// Avvio servizio
        /// </summary>
        /// <param name="args">Argomenti passati da console servizi</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.Information("Esecuzione OnStart");

                List<BatchScheduleConfiguration> schedules = GetScheduleConfiguration();

                InitializeScheduler();
                SetupScheduler(schedules);

                Scheduler.Start();
            }
            catch (Exception err)
            {
                Logger.Error("OnStart error", err);
            }
        }

        /// <summary>
        /// Stop Servizio
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                Logger.Information("Esecuzione OnStop");
                Scheduler.Shutdown();
            }
            catch (Exception err)
            {
                Logger.Error("OnStop error", err);
            }
        }

        /// <summary>
        /// Pausa servizio
        /// </summary>
        protected override void OnPause()
        {
            try
            {
                Logger.Information("Esecuzione OnPause");
                Scheduler.PauseAll();
            }
            catch (Exception err)
            {
                Logger.Error("OnPause error", err);
            }
        }

        /// <summary>
        /// Continua servizio precedentemente fermato in pausa
        /// </summary>
        protected override void OnContinue()
        {
            try
            {
                Logger.Information("Esecuzione OnContinue");
                Scheduler.ResumeAll();
            }
            catch (Exception err)
            {
                Logger.Error("OnContinue error", err);
            }
        }
    }
}
