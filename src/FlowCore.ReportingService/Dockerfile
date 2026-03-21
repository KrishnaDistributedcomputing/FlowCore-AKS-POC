FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FlowCore.Shared/FlowCore.Shared.csproj", "FlowCore.Shared/"]
COPY ["src/FlowCore.ReportingService/FlowCore.ReportingService.csproj", "FlowCore.ReportingService/"]
RUN dotnet restore "FlowCore.ReportingService/FlowCore.ReportingService.csproj"
COPY src/FlowCore.Shared/ FlowCore.Shared/
COPY src/FlowCore.ReportingService/ FlowCore.ReportingService/
WORKDIR /src/FlowCore.ReportingService
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FlowCore.ReportingService.dll"]
