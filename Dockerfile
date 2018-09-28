FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ./*.sln ./NuGet.config ./
COPY ./SchedulerBot.Data/SchedulerBot.Data.csproj ./SchedulerBot.Data/SchedulerBot.Data.csproj
COPY ./SchedulerBot.Client/SchedulerBot.Client.csproj ./SchedulerBot.Client/SchedulerBot.Client.csproj
RUN dotnet restore "./SchedulerBot.Data/SchedulerBot.Data.csproj"
RUN dotnet restore "./SchedulerBot.Client/SchedulerBot.Client.csproj"
COPY ./ ./
RUN dotnet build "./SchedulerBot.Client/SchedulerBot.Client.csproj" -c Release --no-restore
RUN dotnet publish "./SchedulerBot.Client/SchedulerBot.Client.csproj" -c Release -o "../../dist" --no-restore

FROM microsoft/dotnet:2.1-runtime
WORKDIR /
ENTRYPOINT ["dotnet", "SchedulerBot.Client.dll"]
COPY --from=build ./dist .