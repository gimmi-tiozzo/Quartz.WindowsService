
namespace Quartz.WindowsService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione componenti

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.QuartzServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.QuartzServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // QuartzServiceProcessInstaller
            // 
            this.QuartzServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.QuartzServiceProcessInstaller.Password = null;
            this.QuartzServiceProcessInstaller.Username = null;
            // 
            // QuartzServiceInstaller
            // 
            this.QuartzServiceInstaller.Description = "QuartzBatchService - Esempio Windows Service con Quartz .NET";
            this.QuartzServiceInstaller.DisplayName = "QuartzBatchService - Esempio Windows Service con Quartz .NET";
            this.QuartzServiceInstaller.ServiceName = "QuartzBatchService";
            this.QuartzServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.QuartzServiceProcessInstaller,
            this.QuartzServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller QuartzServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller QuartzServiceInstaller;
    }
}