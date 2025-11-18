# Use .Net Core 5 image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy files
COPY ./AccountCommon ./AccountCommon
COPY ./AccountServer ./AccountServer

# Restore and build web with NuGet secrets
RUN --mount=type=secret,id=nuget_username \
    --mount=type=secret,id=nuget_token \
    export NUGET_USERNAME=$(cat /run/secrets/nuget_username) && \
    export NUGET_TOKEN=$(cat /run/secrets/nuget_token) && \
    dotnet restore AccountServer/AccountServer.csproj

# Publish the application
RUN dotnet publish AccountServer/AccountServer.csproj -c Release -o out 

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "AccountServer.dll"]

EXPOSE 8080
