namespace Quartz.WindowsService.Model
{
    /// <summary>
    /// Chiavi di configurazione
    /// </summary>
    public class ConfigurationKeys
    {
        /// <summary>
        /// Chiave per configurazione nome batch quartz
        /// </summary>
        public const string QuartzJobName = "QuartzJobName";

        /// <summary>
        /// Chiave per configurazione sleep in ms per ritentare lettura Database
        /// </summary>
        public const string DbAccessRetrySleep = "DbAccessRetrySleep";

        /// <summary>
        /// Chiave per configurazione per scadenza in ms dei piani di esecuzione in cache
        /// </summary>
        public const string CacheItemPolicyAbsoluteExpiration = "CacheItemPolicyAbsoluteExpiration";

        /// <summary>
        /// Chiave configurazione per abilitare policy scadenza in ms dei piani di esecuzione in cache
        /// </summary>
        public const string EnablePolicyAbsoluteExpiration = "EnablePolicyAbsoluteExpiration";

        /// <summary>
        /// Chiave configurazione che identifica in modo univoco il piano di esecuzione
        /// </summary>
        public const string SchedulePlanKey = "{5EC7F17F-4367-448C-A8A9-56C0150C07F0}";
    }
}
