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

        /// <summary>
        /// Simula un ping a database
        /// </summary>
        public static void PingDatabase()
        {
            using (SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["QuartzDatabase"].ConnectionString))
            {
                cnn.Open();
            }
        }

        /// <summary>
        /// Rimpiazza ultima occorenza di una stringa
        /// </summary>
        /// <param name="find">Stringa da ricercare</param>
        /// <param name="replace">Stringa da rimpiazzare</param>
        /// <param name="str">Stringa completa</param>
        /// <returns>Stringa con replace ultima occorenza</returns>
        public static string ReplaceLast(string find, string replace, string str)
        {
            int lastIndex = str.LastIndexOf(find);

            if (lastIndex == -1)
            {
                return str;
            }

            string beginString = str.Substring(0, lastIndex);
            string endString = str.Substring(lastIndex + find.Length);

            return beginString + replace + endString;
        }

        /// <summary>
        /// Esegui il kill di un processo in base al suo nome
        /// </summary>
        /// <param name="processNames">Nome processi</param>
        public static void KillProcessesByName(string[] processNames)
        {
            foreach (string procName in processNames)
            {
                foreach (var process in Process.GetProcessesByName(procName))
                {
                    process.Kill();
                }
            }
        }
    }
}
