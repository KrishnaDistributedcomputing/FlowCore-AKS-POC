FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FlowCore.Shared/FlowCore.Shared.csproj", "FlowCore.Shared/"]
COPY ["src/FlowCore.NotificationWorker/FlowCore.NotificationWorker.csproj", "FlowCore.NotificationWorker/"]
RUN dotnet restore "FlowCore.NotificationWorker/FlowCore.NotificationWorker.csproj"
COPY src/FlowCore.Shared/ FlowCore.Shared/
COPY src/FlowCore.NotificationWorker/ FlowCore.NotificationWorker/
WORKDIR /src/FlowCore.NotificationWorker
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FlowCore.NotificationWorker.dll"]
