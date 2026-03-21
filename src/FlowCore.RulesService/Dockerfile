FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FlowCore.RulesService/FlowCore.RulesService.csproj", "FlowCore.RulesService/"]
RUN dotnet restore "FlowCore.RulesService/FlowCore.RulesService.csproj"
COPY src/FlowCore.RulesService/ FlowCore.RulesService/
WORKDIR /src/FlowCore.RulesService
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FlowCore.RulesService.dll"]
