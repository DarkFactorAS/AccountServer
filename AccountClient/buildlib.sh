# Clear
rm -Rf bin/debug

# Flush Nuget repo
dotnet nuget locals all -c

# Build and pack common lib
dotnet restore AccountClient.csproj
dotnet build AccountClient.csproj
dotnet pack AccountClient.csproj -o bin/debug

# Push packet
dotnet nuget push bin/debug/DarkFactor.AccountClient.Lib.*.nupkg --api-key 1337 --source DarkFactor --skip-duplicate

read -p "Press any key to resume ..."