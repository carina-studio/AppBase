BUILD_PROJECT_LIST=("Application.Avalonia" "Tests")
PACK_PROJECT_LIST=("Core" "Configuration" "Avalonia" "Application" "Application.Avalonia" "AutoUpdate" "Tests")

# Reset output directory
rm -r ./Packages
mkdir ./Packages
if [ "$?" != "0" ]; then
    exit
fi

# Build projects
for i in "${!BUILD_PROJECT_LIST[@]}"; do
    PROJECT=${BUILD_PROJECT_LIST[$i]}

    dotnet build $PROJECT -c Release
    if [ "$?" != "0" ]; then
        exit
    fi
done

# Pack projects
for i in "${!PACK_PROJECT_LIST[@]}"; do
    PROJECT=${PACK_PROJECT_LIST[$i]}

    dotnet pack $PROJECT -c Release -o ./Packages
    if [ "$?" != "0" ]; then
        exit
    fi
done