@echo off

setlocal EnableDelayedExpansion

set DEFAULT_PACK_PROJECT_LIST=Core Configuration Avalonia Application Application.Avalonia AutoUpdate MacOS Tests
set TEST_PROJECT_LIST=Core.Tests Configuration.Tests Tests.Tests Application.Tests AutoUpdate.Tests Avalonia.Tests
set BUILD_PROJECT_LIST=Application.Avalonia MacOS Tests
set PACK_PROJECT_LIST=
set CONFIG=Release
set FRAMEWORK=
set OUTPUT_DIR=Packages
set RUN_TESTS=true
set BUILD_PACKAGES=true

REM Parse arguments
:parse_arguments
if "%~1"=="" goto arguments_parsed
if /I "%~1"=="-h" goto print_usage_and_exit
if /I "%~1"=="--help" goto print_usage_and_exit
if /I "%~1"=="--project" goto parse_project_argument
if /I "%~1"=="--config" (
    if "%~2"=="" (
        echo Missing value of '--config'
        exit /b 1
    )
    set CONFIG=%~2
    shift
    shift
    goto parse_arguments
)
if /I "%~1"=="--framework" (
    if "%~2"=="" (
        echo Missing value of '--framework'
        exit /b 1
    )
    set FRAMEWORK=%~2
    shift
    shift
    goto parse_arguments
)
if /I "%~1"=="--output" (
    if "%~2"=="" (
        echo Missing value of '--output'
        exit /b 1
    )
    set OUTPUT_DIR=%~2
    shift
    shift
    goto parse_arguments
)
if /I "%~1"=="--no-tests" (
    set RUN_TESTS=false
    shift
    goto parse_arguments
)
if /I "%~1"=="--tests-only" (
    set BUILD_PACKAGES=false
    shift
    goto parse_arguments
)
echo Unknown argument: %~1
echo.
call :print_usage
exit /b 1

REM Select the project to build package for
:parse_project_argument
if "%~2"=="" (
    echo Missing value of '--project'
    exit /b 1
)
set IS_PROJECT_SUPPORTED=false
for %%p in (%DEFAULT_PACK_PROJECT_LIST%) do (
    if /I "%%p"=="%~2" set IS_PROJECT_SUPPORTED=true
)
if "%IS_PROJECT_SUPPORTED%"=="false" (
    echo Unsupported project: %~2
    echo Supported projects: %DEFAULT_PACK_PROJECT_LIST%
    exit /b 1
)
set IS_PROJECT_SELECTED=false
if not "%PACK_PROJECT_LIST%"=="" (
    for %%p in (%PACK_PROJECT_LIST%) do (
        if /I "%%p"=="%~2" set IS_PROJECT_SELECTED=true
    )
)
if "%IS_PROJECT_SELECTED%"=="false" (
    if "%PACK_PROJECT_LIST%"=="" (
        set PACK_PROJECT_LIST=%~2
    ) else (
        set PACK_PROJECT_LIST=!PACK_PROJECT_LIST! %~2
    )
)
shift
shift
goto parse_arguments

REM Print usage of this script and exit
:print_usage_and_exit
call :print_usage
exit /b 0

REM Print usage of this script
:print_usage
echo Usage: BuildPackages.bat [options]
echo.
echo Options:
echo   -h, --help          Print this help message and exit.
echo   --config ^<name^>     Build configuration to use. (Default: %CONFIG%)
echo   --framework ^<tfm^>   Target framework to run test cases on. (Default: all of them)
echo   --output ^<dir^>      Directory to place built packages into. (Default: %OUTPUT_DIR%)
echo   --project ^<name^>    Project to build package for, can be specified multiple times.
echo                       Supported: %DEFAULT_PACK_PROJECT_LIST%. (Default: all of them)
echo   --no-tests          Do not run test cases before building packages.
echo   --tests-only        Only run test cases, do not build packages.
goto :eof

REM Select all projects if none of them was specified
:arguments_parsed
set IS_FULL_BUILD=true
if "%PACK_PROJECT_LIST%"=="" (
    set PACK_PROJECT_LIST=%DEFAULT_PACK_PROJECT_LIST%
) else (
    set IS_FULL_BUILD=false
)

REM Get version of packages
set VERSION_LINE=
for /f "delims=" %%l in ('findstr /C:"<PackageVersion>" Directory.Build.props') do set "VERSION_LINE=%%l"
if "!VERSION_LINE!"=="" (
    echo Unable to get version of packages from 'Directory.Build.props'
    exit /b 1
)
set "PACKAGE_VERSION=!VERSION_LINE:*<PackageVersion>=!"
set "PACKAGE_VERSION=!PACKAGE_VERSION:</PackageVersion>=!"

echo ********** Start building AppBase !PACKAGE_VERSION! **********

REM Run test cases
if /I "%RUN_TESTS%"=="true" (
    echo.
    echo Skip test cases of MacOS.Tests which can only run on macOS
    for %%p in (%TEST_PROJECT_LIST%) do (
        echo.
        echo [Test cases of %%p]
        echo.
        if "%FRAMEWORK%"=="" (
            dotnet test %%p -c %CONFIG%
        ) else (
            dotnet test %%p -c %CONFIG% -f %FRAMEWORK%
        )
        if !ERRORLEVEL! neq 0 (
            echo Test cases of %%p failed
            exit /b 1
        )
    )
)
if /I "%BUILD_PACKAGES%"=="false" exit /b 0

REM Reset output directory
if /I "%IS_FULL_BUILD%"=="true" (
    if exist "%OUTPUT_DIR%" (
        echo.
        echo Clear directory '%OUTPUT_DIR%'
        rmdir "%OUTPUT_DIR%" /s /q
        if !ERRORLEVEL! neq 0 exit /b 1
    )
)
if not exist "%OUTPUT_DIR%" (
    echo Create directory '%OUTPUT_DIR%'
    mkdir "%OUTPUT_DIR%"
    if !ERRORLEVEL! neq 0 exit /b 1
)

REM Build projects
for %%p in (%BUILD_PROJECT_LIST%) do (
    echo.
    echo [Build %%p]
    echo.
    dotnet build %%p -c %CONFIG%
    if !ERRORLEVEL! neq 0 (
        echo Failed to build %%p
        exit /b 1
    )
)

REM Pack projects
for %%p in (%PACK_PROJECT_LIST%) do (
    echo.
    echo [Pack %%p]
    echo.
    dotnet pack %%p -c %CONFIG% -o "%OUTPUT_DIR%"
    if !ERRORLEVEL! neq 0 (
        echo Failed to pack %%p
        exit /b 1
    )
)
