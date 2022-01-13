# Versione 1.0.0.0
# Utility di installazione Servizio Windows Quartz

[CmdletBinding()]
param(

    [Parameter (Mandatory=$true,HelpMessage="Indicare la fase di esecuzione. Fase: [Install|Uninstall]")][ValidateSet('Install','Uninstall')][System.String]$Fase

)

# file di log
$logFile = "D:\QuartzBatchServiceInstall.log"
# codice di ritorno
$returnCode = 0;

# Metodo di logging
# $logstring: stringa tracciata nei log
Function LogWrite {
    param (
        [Parameter (Mandatory=$true,Position=0,HelpMessage="Stringa del messaggio di log")][String]$logstring
    )
	
    $logstring = "$((Get-Date).ToString()) | $logstring"
    Add-content $logFile -value $logstring
}

#esegui un comando da shell
# Command: comando
# Arguments: argomenti da linea di comando
# ReturnCodes: nnnn - Il return code del processo
function executeCommand {
    param(
        [Parameter (Mandatory=$true,Position=1,HelpMessage="comando da eseguire")][String]$Command,
        [Parameter (Mandatory=$false,Position=2,HelpMessage="stringa contenete gli argomenti da passare al comando")][String]$Arguments
    )

    LogWrite "Comando da eseguire: $Command"
    LogWrite "Argomenti: $Arguments"
    $returnCode = 0;

    if ($Arguments -eq $null)
    {
        $process = start-process "$Command" -PassThru -WindowStyle hidden
    }
    else
    {
        $process = start-process "$Command" -arg $Arguments -PassThru -WindowStyle hidden
    }

    Wait-Process -InputObject $process
    return $process.ExitCode;
}

if ($Fase -eq "Install")
{
    LogWrite " ";
    LogWrite "----- START FASE DI INSTALL -----";
    LogWrite "Step1. Verifico se è stato eseguito il porting del servizio da change console, ovvero se sono presenti i file nel FileSystem";

    $returnCode = Test-Path -Path "D:\QuartzBatchService\Quartz.WindowsService.exe";

    if ($returnCode -ne 1)
    {
        LogWrite "!!! ERRORE Nel Check presenza file Servizio Quartz nel FileSystem. File non trovati. ReturnCode: $($returnCode) !!!";
        LogWrite "----- STOP PACCHETTO DI INSTALLAZIONE  -----";
        LogWrite " ";

        exit 10;
    }

    LogWrite "File servizio Quartz trovato nel FileSystem. Si prosegue con il resto dell'installazione";
    LogWrite "Step2. Installazione del servizio tramite installutil.exe";

    $Command = "C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe";
	$Args = "/ShowCallStack `"D:\QuartzBatchService\Quartz.WindowsService.exe`" /DisplayName=`"QuartzBatchService - Esempio Windows Service con Quartz .NET`"";

    $returnCode = executeCommand -Command "$($Command)" -Arguments "$($Args)";

    if ($returnCode -ne 0)
    {
        LogWrite "!!! ERRORE nell'esecuzione del comando di installutil.exe. ReturnCode: $($returnCode) !!!";
		LogWrite "----- STOP PACCHETTO DI INSTALLAZIONE -----";
		LogWrite " ";

        exit 20;
    }

    LogWrite "Installazione terminata con successo";

    #tutto OK
    exit 0
}

if ($Fase -eq "Uninstall")
{
	LogWrite " ";
	LogWrite "----- START FASE DI UNINSTALL -----";

    LogWrite "Step1. Verifico se i file del servizio Quartz esistono nel FileSystem";

    $returnCode = Test-Path -Path "D:\QuartzBatchService\Quartz.WindowsService.exe";

    if ($returnCode -ne 1)
    {
        LogWrite "!!! ERRORE Nel Check presenza file Servizio Quartz nel FileSystem. File non trovati. ReturnCode: $($returnCode) !!!";
        LogWrite "----- STOP PACCHETTO DI DISINSTALLAZIONE -----";
        LogWrite " ";

        exit 10;
    }

    LogWrite "File servizio Quartz trovato nel FileSystem. Si prosegue con il resto della disinstallazione";
    LogWrite "Step2. Disinstallazione del servizio tramite installutil.exe";

    $Command = "C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe";
	$Args = "/u /ShowCallStack `"D:\QuartzBatchService\Quartz.WindowsService.exe`" /DisplayName=`"QuartzBatchService - Esempio Windows Service con Quartz .NET`"";

    $returnCode = executeCommand -Command "$($Command)" -Arguments "$($Args)";

    if ($returnCode -ne 0)
    {
        LogWrite "!!! ERRORE nell'esecuzione del comando di installutil.exe. ReturnCode: $($returnCode) !!!";
		LogWrite "----- STOP PACCHETTO DI INSTALLAZIONE -----";
		LogWrite " ";

        exit 20;
    }

    LogWrite "Disinstallazione terminata con successo";

    #tutto OK
    exit 0
}