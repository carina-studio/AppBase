#!/bin/bash

DEFAULT_PACK_PROJECT_LIST=("Core" "Configuration" "Avalonia" "Application" "Application.Avalonia" "AutoUpdate" "MacOS" "Tests")
TEST_PROJECT_LIST=("Core.Tests" "Configuration.Tests" "Tests.Tests" "Application.Tests" "AutoUpdate.Tests" "Avalonia.Tests" "MacOS.Tests")
MACOS_ONLY_TEST_PROJECT_LIST=("MacOS.Tests")
BUILD_PROJECT_LIST=("Application.Avalonia" "MacOS" "Tests")
PACK_PROJECT_LIST=()
CONFIG="Release"
FRAMEWORK=""
OUTPUT_DIR="./Packages"
RUN_TESTS="true"
BUILD_PACKAGES="true"

# Print usage of this script.
print_usage() {
    echo "Usage: BuildPackages.sh [options]"
    echo " "
    echo "Options:"
    echo "  -h, --help          Print this help message and exit."
    echo "  --config <name>     Build configuration to use. (Default: $CONFIG)"
    echo "  --framework <tfm>   Target framework to run test cases on. (Default: all of them)"
    echo "  --output <dir>      Directory to place built packages into. (Default: $OUTPUT_DIR)"
    echo "  --project <name>    Project to build package for, can be specified multiple times."
    echo "                      Supported: ${DEFAULT_PACK_PROJECT_LIST[*]}. (Default: all of them)"
    echo "  --no-tests          Do not run test cases before building packages."
    echo "  --tests-only        Only run test cases, do not build packages."
}

# Parse arguments
while [ $# -gt 0 ]; do
    case "$1" in
        -h|--help)
            print_usage
            exit 0
            ;;
        --config)
            if [ -z "$2" ]; then
                echo "Missing value of '--config'"
                exit 1
            fi
            CONFIG="$2"
            shift 2
            ;;
        --framework)
            if [ -z "$2" ]; then
                echo "Missing value of '--framework'"
                exit 1
            fi
            FRAMEWORK="$2"
            shift 2
            ;;
        --output)
            if [ -z "$2" ]; then
                echo "Missing value of '--output'"
                exit 1
            fi
            OUTPUT_DIR="$2"
            shift 2
            ;;
        --project)
            if [ -z "$2" ]; then
                echo "Missing value of '--project'"
                exit 1
            fi
            IS_PROJECT_SUPPORTED="false"
            for SUPPORTED_PROJECT in "${DEFAULT_PACK_PROJECT_LIST[@]}"; do
                if [ "$SUPPORTED_PROJECT" = "$2" ]; then
                    IS_PROJECT_SUPPORTED="true"
                    break
                fi
            done
            if [ "$IS_PROJECT_SUPPORTED" = "false" ]; then
                echo "Unsupported project: $2"
                echo "Supported projects: ${DEFAULT_PACK_PROJECT_LIST[*]}"
                exit 1
            fi
            IS_PROJECT_SELECTED="false"
            for SELECTED_PROJECT in "${PACK_PROJECT_LIST[@]}"; do
                if [ "$SELECTED_PROJECT" = "$2" ]; then
                    IS_PROJECT_SELECTED="true"
                    break
                fi
            done
            if [ "$IS_PROJECT_SELECTED" = "false" ]; then
                PACK_PROJECT_LIST+=("$2")
            fi
            shift 2
            ;;
        --no-tests)
            RUN_TESTS="false"
            shift
            ;;
        --tests-only)
            BUILD_PACKAGES="false"
            shift
            ;;
        *)
            echo "Unknown argument: $1"
            echo " "
            print_usage
            exit 1
            ;;
    esac
done

# Select all projects if none of them was specified
IS_FULL_BUILD="true"
if [ ${#PACK_PROJECT_LIST[@]} -eq 0 ]; then
    PACK_PROJECT_LIST=("${DEFAULT_PACK_PROJECT_LIST[@]}")
else
    IS_FULL_BUILD="false"
fi

# Get version of packages
PACKAGE_VERSION=$(sed -n 's/.*<PackageVersion>\(.*\)<\/PackageVersion>.*/\1/p' ./Directory.Build.props)
if [ -z "$PACKAGE_VERSION" ]; then
    echo "Unable to get version of packages from 'Directory.Build.props'"
    exit 1
fi

echo "********** Start building AppBase $PACKAGE_VERSION **********"

# Run test cases
if [ "$RUN_TESTS" = "true" ]; then
    for i in "${!TEST_PROJECT_LIST[@]}"; do
        PROJECT=${TEST_PROJECT_LIST[$i]}

        # skip test cases which can only run on macOS
        IS_MACOS_ONLY_PROJECT="false"
        for MACOS_ONLY_PROJECT in "${MACOS_ONLY_TEST_PROJECT_LIST[@]}"; do
            if [ "$MACOS_ONLY_PROJECT" = "$PROJECT" ]; then
                IS_MACOS_ONLY_PROJECT="true"
                break
            fi
        done
        if [ "$IS_MACOS_ONLY_PROJECT" = "true" ] && [ "$(uname)" != "Darwin" ]; then
            echo " "
            echo "Skip test cases of $PROJECT on $(uname)"
            continue
        fi

        echo " "
        echo "[Test cases of $PROJECT]"
        echo " "

        if [ -z "$FRAMEWORK" ]; then
            dotnet test "$PROJECT" -c "$CONFIG"
        else
            dotnet test "$PROJECT" -c "$CONFIG" -f "$FRAMEWORK"
        fi
        if [ "$?" != "0" ]; then
            echo "Test cases of $PROJECT failed"
            exit 1
        fi
    done
fi
if [ "$BUILD_PACKAGES" = "false" ]; then
    exit 0
fi

# Reset output directory
if [ "$IS_FULL_BUILD" = "true" ] && [ -d "$OUTPUT_DIR" ]; then
    echo " "
    echo "Clear directory '$OUTPUT_DIR'"
    rm -r "$OUTPUT_DIR"
    if [ "$?" != "0" ]; then
        exit 1
    fi
fi
if [ ! -d "$OUTPUT_DIR" ]; then
    echo "Create directory '$OUTPUT_DIR'"
    mkdir -p "$OUTPUT_DIR"
    if [ "$?" != "0" ]; then
        exit 1
    fi
fi

# Build projects
for i in "${!BUILD_PROJECT_LIST[@]}"; do
    PROJECT=${BUILD_PROJECT_LIST[$i]}

    echo " "
    echo "[Build $PROJECT]"
    echo " "

    dotnet build "$PROJECT" -c "$CONFIG"
    if [ "$?" != "0" ]; then
        echo "Failed to build $PROJECT"
        exit 1
    fi
done

# Pack projects
for i in "${!PACK_PROJECT_LIST[@]}"; do
    PROJECT=${PACK_PROJECT_LIST[$i]}

    echo " "
    echo "[Pack $PROJECT]"
    echo " "

    dotnet pack "$PROJECT" -c "$CONFIG" -o "$OUTPUT_DIR"
    if [ "$?" != "0" ]; then
        echo "Failed to pack $PROJECT"
        exit 1
    fi
done
