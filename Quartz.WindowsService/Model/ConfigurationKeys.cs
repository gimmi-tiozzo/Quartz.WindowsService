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
    }
}
