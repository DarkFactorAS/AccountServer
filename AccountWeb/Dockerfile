# Build from the repository root:  docker build -f AccountWeb/Dockerfile .
FROM node:22-alpine AS client
WORKDIR /client
COPY AccountWeb/package.json AccountWeb/package-lock.json AccountWeb/vite.config.js AccountWeb/index.html ./
COPY AccountWeb/js/ ./js/
COPY AccountWeb/css/ ./css/
RUN npm ci
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY ./AccountCommon ./AccountCommon
COPY ./AccountClient ./AccountClient
COPY ./AccountWeb ./AccountWeb
RUN --mount=type=secret,id=nuget_username \
    --mount=type=secret,id=nuget_token \
    export NUGET_USERNAME=$(cat /run/secrets/nuget_username) && \
    export NUGET_TOKEN=$(cat /run/secrets/nuget_token) && \
    dotnet restore AccountWeb/AccountWeb.csproj
RUN dotnet publish AccountWeb/AccountWeb.csproj -c Release --no-restore -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
COPY --from=client /client/dist/ ./wwwroot/
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "AccountWeb.dll"]
