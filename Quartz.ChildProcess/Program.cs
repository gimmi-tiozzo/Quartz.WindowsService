using Quartz.Common;
using System;
using System.Security.Principal;

namespace Quartz.ChildProcess
{
    class Program
    {
        /// <summary>
        /// Loggert
        /// </summary>
        private static readonly BatchLogger Logger = BatchLogger.GetLogger();

        /// <summary>
        /// Entry Point
        /// </summary>
        /// <param name="args">Argomenti da linea di comando</param>
        static void Main(string[] args)
        {
            try
            {
                string argument = args.Length == 1 ? args[0] : "Argomento Assente!!";
                Logger.Information($"Questo è un messaggio di test. UserInteractive: {Environment.UserInteractive} - Argomento: {argument} - User: {WindowsIdentity.GetCurrent().Name}");

                //esegui ping al database in autenticazione integrata
                Utilities.PingDatabase();
                Logger.Information("Aperta e chiusa connessione al database da processo Child");
            }
            catch (Exception err)
            {
                Logger.Error("Errore processo Root", err);
            }
        }
    }
}
