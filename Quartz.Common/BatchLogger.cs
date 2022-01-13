using log4net;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Quartz.Common
{
    /// <summary>
    /// Classi di logging
    /// </summary>
    public class BatchLogger
    {
        /// <summary>
        /// Token di sessione
        /// </summary>
        private static string SessionToken { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        private static ILog Logger { get; set; }

        /// <summary>
        /// Costruttore statico
        /// </summary>
        static BatchLogger()
        {
            if (String.IsNullOrWhiteSpace(SessionToken))
            {
                SessionToken = Guid.NewGuid().ToString();
            }

            log4net.Config.XmlConfigurator.Configure();
            Logger = LogManager.GetLogger("BatchLogger");
        }

        /// <summary>
        /// Ottieni un Logger per una classe
        /// </summary>
        /// <returns>Logger per una classe</returns>
        public static BatchLogger GetLogger()
        {
            return new BatchLogger();
        }

        /// <summary>
        /// Costruisci un messaggio con un token di sessione
        /// </summary>
        /// <param name="message">Messaggio testuale</param>
        /// <param name="className">Nome classe</param>
        /// <param name="methodName">Nome metodo</param>
        /// <returns>Messaggio = Token sessione + Messaggio da tracciare</returns>
        private string GetMessage(string message, string className, string methodName)
        {
            string codeRerefence = $"[{className}::{methodName}]".PadRight(50, ' ');
            return SessionToken + " - " + codeRerefence + message;
        }

        /// <summary>
        /// Traccia un errore
        /// </summary>
        /// <param name="message">Messaggio testuale</param>
        /// <param name="err">Eccezione</param>
        public void Error(string message, Exception err)
        {
            //calcola nome metodo e classe chiamante
            MethodBase methodBase = new StackTrace().GetFrame(1).GetMethod();
            string methodName = methodBase.Name;
            string className = methodBase.ReflectedType.Name;

            if (err != null)
            {
                Logger.Error(GetMessage(message, className, methodName), err);
            }
            else
            {
                Logger.Error(GetMessage(message, className, methodName));
            }
        }

        /// <summary>
        /// Traccia un warning
        /// </summary>
        /// <param name="message">Messaggio testuale</param>
        public void Warning(string message)
        {
            //calcola nome metodo e classe chiamante
            MethodBase methodBase = new StackTrace().GetFrame(1).GetMethod();
            string methodName = methodBase.Name;
            string className = methodBase.ReflectedType.Name;

            Logger.Warn(GetMessage(message, className, methodName));
        }

        /// <summary>
        /// Traccia Information
        /// </summary>
        /// <param name="message">Messaggio testuale</param>
        public void Information(string message)
        {
            //calcola nome metodo e classe chiamante
            MethodBase methodBase = new StackTrace().GetFrame(1).GetMethod();
            string methodName = methodBase.Name;
            string className = methodBase.ReflectedType.Name;

            Logger.Info(GetMessage(message, className, methodName));
        }
    }
}
