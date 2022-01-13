using System;
using System.IO;

namespace Quartz.RootProcess
{
    /// <summary>
    /// Entry Point
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry Point
        /// </summary>
        /// <param name="args">Argomenti da linea di comando</param>
        static void Main(string[] args)
        {
            try
            {
                using (FileStream file = new FileStream(@"D:\RootProcess.log", FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        string argument = args.Length == 1 ? args[0] : "Argomento Assente!!";
                        writer.WriteLine(String.Format("{0,-28} - {1,-12} - {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "INFO", $"Questo è un messaggio di test. UserInteractive: {Environment.UserInteractive} - Argomento: {argument}"));
                    }
                }
            }
            catch
            {
                //NOP: è un log pensato per lo sviluppo\debug
            }
        }
    }
}
