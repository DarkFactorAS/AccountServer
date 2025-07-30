# Use .Net Core 5 image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Get the NuGet arguments and set as environment to use in NuGet
ARG username
ARG token
ENV NUGET_USERNAME=$username
ENV NUGET_TOKEN=$token

# Copy files
COPY ./AccountCommon ./AccountCommon
COPY ./AccountServer ./AccountServer

# Copy nuget file
#COPY ./AccountServer/NuGet_Docker.config ./AccountServer/NuGet.config

# Restore and build web
RUN dotnet restore AccountServer/AccountServer.csproj
RUN dotnet publish AccountServer/AccountServer.csproj -c Release -o out

# Create self signed sertificate
RUN dotnet dev-certs https -ep /app/out/certificate.pfx -p smurfepoliz
RUN dotnet dev-certs https --trust

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "AccountServer.dll"]

EXPOSE 5100:8080
