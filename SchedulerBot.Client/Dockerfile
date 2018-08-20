FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY SchedulerBot.Client/SchedulerBot.Client.csproj SchedulerBot.Client/
COPY SchedulerBot.Data/SchedulerBot.Data.csproj SchedulerBot.Data/
RUN dotnet restore SchedulerBot.Client/SchedulerBot.Client.csproj
COPY . .
WORKDIR /src/SchedulerBot.Client
RUN dotnet build SchedulerBot.Client.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish SchedulerBot.Client.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SchedulerBot.Client.dll"]
