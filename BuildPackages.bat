@echo off

set JDK_DIR=
set ANDROID_SDK_DIR=

IF exist Packages (
	rmdir Packages /s /q
)
mkdir Packages

dotnet build Application.Android -c Release "/p:AndroidSdkDirectory=%ANDROID_SDK_DIR%" "/p:JavaSdkDirectory=%JDK_DIR%"
IF %ERRORLEVEL% NEQ 0 ( 
   exit
)

dotnet build Application.Avalonia -c Release
IF %ERRORLEVEL% NEQ 0 ( 
   exit
)

dotnet build MacOS -c Release
IF %ERRORLEVEL% NEQ 0 ( 
   exit
)

dotnet build Tests -c Release
IF %ERRORLEVEL% NEQ 0 ( 
   exit
)

dotnet pack Core -c Release -o Packages
dotnet pack Configuration -c Release -o Packages
dotnet pack Avalonia -c Release -o Packages
dotnet pack Application -c Release -o Packages
dotnet pack Application.Avalonia -c Release -o Packages
dotnet pack AutoUpdate -c Release -o Packages
dotnet pack MacOS -c Release -o Packages
dotnet pack Tests -c Release -o Packages