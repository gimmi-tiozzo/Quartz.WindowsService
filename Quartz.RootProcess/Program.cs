﻿using Quartz.Common;
using System;
using System.Security.Principal;

namespace Quartz.RootProcess
{
    /// <summary>
    /// Entry Point
    /// </summary>
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
            string argument = args.Length == 1 ? args[0] : "Argomento Assente!!";
            Logger.Information($"Questo è un messaggio di test. UserInteractive: {Environment.UserInteractive} - Argomento: {argument} - User: {WindowsIdentity.GetCurrent().Name}");

            //lancio processo figlio in modalità headeless
            Utilities.HeadelessProcessStart("Quartz.ChildProcess.exe", "child_param");
        }
    }
}
