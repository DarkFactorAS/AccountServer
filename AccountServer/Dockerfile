# Use .Net Core 3.1 image
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Add nuget repository
#COPY ./NuGet/DarkFactor.Common.Lib.*.nupkg ./NuGet/
#RUN dotnet nuget add source /app/NuGet --name DarkFactor

# Flush all nuget repos
RUN dotnet nuget locals all -c

# Copy files
COPY ./ ./
#COPY ./NuGet_Docker.config ./NuGet.config

# Restore and build web
RUN dotnet restore AccountServer.csproj
RUN dotnet publish AccountServer.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "AccountServer.dll"]

EXPOSE 5100:80
