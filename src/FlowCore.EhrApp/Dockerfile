FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FlowCore.EhrApp/FlowCore.EhrApp.csproj", "FlowCore.EhrApp/"]
RUN dotnet restore "FlowCore.EhrApp/FlowCore.EhrApp.csproj"
COPY src/FlowCore.EhrApp/ FlowCore.EhrApp/
WORKDIR /src/FlowCore.EhrApp
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FlowCore.EhrApp.dll"]
