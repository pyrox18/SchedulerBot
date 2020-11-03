FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ./*.sln ./NuGet.config ./
COPY ./SchedulerBot.Data/SchedulerBot.Data.csproj ./SchedulerBot.Data/SchedulerBot.Data.csproj
COPY ./SchedulerBot.Client/SchedulerBot.Client.csproj ./SchedulerBot.Client/SchedulerBot.Client.csproj
RUN dotnet restore "./SchedulerBot.Data/SchedulerBot.Data.csproj"
RUN dotnet restore "./SchedulerBot.Client/SchedulerBot.Client.csproj"
COPY ./ ./
RUN dotnet build "./SchedulerBot.Client/SchedulerBot.Client.csproj" -c Release --no-restore
RUN dotnet publish "./SchedulerBot.Client/SchedulerBot.Client.csproj" -c Release -o "../../dist" --no-restore

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /
ENTRYPOINT ["dotnet", "SchedulerBot.Client.dll"]
COPY --from=build ./dist .
