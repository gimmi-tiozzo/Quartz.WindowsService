#funzione per la build automatizzata
function buildSolution
{
	param
    (
		#path del file di solution
        [parameter(Mandatory=$true)]
        [String] $path,

		#indica se eseguire il restore automatico dei package tramite nuget
        [parameter(Mandatory=$false)]
        [bool] $nuget = $true,
        
		#indica se eseguire la pulitura prima della build
        [parameter(Mandatory=$false)]
        [bool] $clean = $true
    )
	process
    {
		#path msbuild usato da Visual Studio 2017
        #$msBuildExe = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe'
		
		#path msbuild usato da Visual Studio 2019 Community
		$msBuildExe = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe'
		
		if ($nuget) {
            Write-Host "Ripristino pacchetti NuGet" -foregroundcolor green
            nuget restore "$($path)"
        }
		
        if ($clean) {
            Write-Host "Pulitura pre-build $($path)" -foregroundcolor green
            & "$($msBuildExe)" "$($path)" /t:Clean /m
        }

        Write-Host "Build $($path)" -foregroundcolor green
        & "$($msBuildExe)" "$($path)" /t:Build /m /p:Configuration=Release
		
		#cartella in cui viene copiata la struttura di rilascio
		$releaseDir="D:\QuartzBatchService"
		
		#se esiste la cartella di release allora la cancelli per poi ricrearla
		If(Test-Path $releaseDir)
		{
			Remove-Item $releaseDir -Recurse
		}
		
		New-Item -ItemType Directory -Force -Path $releaseDir
		
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Common.Logging.Core.dll -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Common.Logging.dll -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\log4net.dll -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Quartz.dll -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Quartz.RootProcess.exe -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Quartz.ChildProcess.exe -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Quartz.WindowsService.exe -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Quartz.Common.dll -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Quartz.WindowsService.exe.config -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Quartz.RootProcess.exe.config -Destination $releaseDir
		Copy-Item -Path ..\Quartz.WindowsService\bin\Release\Quartz.ChildProcess.exe.config -Destination $releaseDir
		Copy-Item -Path ..\Tools\service-util.ps1 -Destination $releaseDir
	}
}

#lancia la build della solution com il deploy in modalita' release
buildSolution -path ..\Quartz.WindowsService.sln -nuget $false -clean $true