@echo off
REM Clear
rd /q /s .\bin\debug

REM Flush Nuget repo
dotnet nuget locals all -c

REM Build and pack common lib
dotnet restore AccountClient.csproj
dotnet build AccountClient.csproj
dotnet pack AccountClient.csproj -o bin/debug

REM Push packet
dotnet nuget push .\bin\debug\DarkFactor.AccountClient.Lib.*.nupkg --api-key 1337 --source DarkFactor --skip-duplicate

read -p "Press any key to resume ..."