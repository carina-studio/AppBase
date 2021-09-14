@echo off

IF not exist Packages (
	mkdir Packages
)

dotnet build Application.Avalonia -c Release
IF %ERRORLEVEL% NEQ 0 ( 
   exit
)

dotnet build Tests -c Release
IF %ERRORLEVEL% NEQ 0 ( 
   exit
)

dotnet pack Core -c Release -o Packages
dotnet pack Configuration -c Release -o Packages
dotnet pack Application -c Release -o Packages
dotnet pack Application.Avalonia -c Release -o Packages
dotnet pack AutoUpdate -c Release -o Packages
dotnet pack Tests -c Release -o Packages