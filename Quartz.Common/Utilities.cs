using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Quartz.Common
{
    /// <summary>
    /// Classe di utilitià
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Lancia un processo in modalità headless
        /// </summary>
        /// <param name="path">Path processo</param>
        /// <param name="arguments">Argomenti processo</param>
        public static void HeadelessProcessStart(string path, string arguments)
        {
            //avvia il processo in modalità senza shell
            ProcessStartInfo headelessProcess = new ProcessStartInfo()
            {
                FileName = path,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(headelessProcess);
        }

        public static void PingDatabase()
        {
            using (SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["QuartzDatabase"].ConnectionString))
            {
                cnn.Open();
            }
        }

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
    }
}
