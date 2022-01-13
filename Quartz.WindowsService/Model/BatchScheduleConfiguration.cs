namespace Quartz.WindowsService.Model
{
    /// <summary>
    /// Entity che rappresenta una configurazione di schedulazione
    /// </summary>
    public class BatchScheduleConfiguration
    {
        /// <summary>
        /// Id regola di schedulazione
        /// </summary>
        public string IdRule { get; set; }

        /// <summary>
        /// Espressione Cron
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// Nome batch
        /// </summary>
        public string BatchName { get; set; }

        /// <summary>
        /// Nome server
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Path processo
        /// </summary>
        public string ProcessPath { get; set; }

        /// <summary>
        /// Parametri Processo
        /// </summary>
        public string ProcessParameters { get; set; }

        /// <summary>
        /// Ottieni rappresentazione oggetto in formato stringa
        /// </summary>
        /// <returns>Rappresentazione oggetto in formato stringa</returns>
        public override string ToString()
        {
            return $"IdRule: {IdRule}, CronExpression: {CronExpression}, BatchName: {BatchName}, ServerName: {ServerName}, ProcessPath: {ProcessPath}, ProcessParameters: {ProcessParameters}";
        }
    }
}
