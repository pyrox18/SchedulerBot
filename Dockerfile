FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ./*.sln ./NuGet.config ./
COPY ./SchedulerBot.Domain/SchedulerBot.Domain.csproj ./SchedulerBot.Domain/SchedulerBot.Domain.csproj
COPY ./SchedulerBot.Application/SchedulerBot.Application.csproj ./SchedulerBot.Application/SchedulerBot.Application.csproj
COPY ./SchedulerBot.Persistence/SchedulerBot.Persistence.csproj ./SchedulerBot.Persistence/SchedulerBot.Persistence.csproj
COPY ./SchedulerBot.Infrastructure/SchedulerBot.Infrastructure.csproj ./SchedulerBot.Infrastructure/SchedulerBot.Infrastructure.csproj
COPY ./SchedulerBot.Client/SchedulerBot.Client.csproj ./SchedulerBot.Client/SchedulerBot.Client.csproj
RUN dotnet restore "./SchedulerBot.Domain/SchedulerBot.Domain.csproj"
RUN dotnet restore "./SchedulerBot.Application/SchedulerBot.Application.csproj"
RUN dotnet restore "./SchedulerBot.Persistence/SchedulerBot.Persistence.csproj"
RUN dotnet restore "./SchedulerBot.Infrastructure/SchedulerBot.Infrastructure.csproj"
RUN dotnet restore "./SchedulerBot.Client/SchedulerBot.Client.csproj"
COPY ./ ./
RUN dotnet build "./SchedulerBot.Client/SchedulerBot.Client.csproj" -c Release --no-restore
RUN dotnet publish "./SchedulerBot.Client/SchedulerBot.Client.csproj" -c Release -o "../../dist" --no-restore

FROM microsoft/dotnet:2.2-runtime
WORKDIR /
ENTRYPOINT ["dotnet", "SchedulerBot.Client.dll"]
COPY --from=build ./dist .
