@echo off

SET OUTPUTDIR=%1
:: this script needs https://www.nuget.org/packages/ilmerge

SET PROJECT_NAME=RSDiagnostics

:: set your target executable name (typically [projectname].exe)
SET APP_NAME=%PROJECT_NAME%.exe

:: set your NuGet ILMerge Version, this is the number from the package manager install, for example:
:: PM> Install-Package ilmerge -Version 3.0.41
:: to confirm it is installed for a given project, see the packages.config file
SET ILMERGE_VERSION=3.0.41

:: the full ILMerge should be found here:
SET ILMERGE_PATH=%USERPROFILE%\.nuget\packages\ilmerge\%ILMERGE_VERSION%\tools\net452
:: dir "%ILMERGE_PATH%"\ILMerge.exe

echo ILMerge: %APP_NAME% ...

cd ../../

:: add project DLL's starting with replacing the FirstLib with this project's DLL
"%ILMERGE_PATH%"\ILMerge.exe %OUTPUTDIR%/%APP_NAME%  ^
  /lib:./ ^
  /out:%APP_NAME% ^
  Resources\BouncyCastle.Crypto.dll ^
  Resources\Newtonsoft.Json.dll ^
  Resources\Pfim.dll ^
  Resources\Rocksmith2014PsarcLib.dll

move %APP_NAME% %OUTPUTDIR%/%APP_NAME% >nul
del %PROJECT_NAME%.pdb