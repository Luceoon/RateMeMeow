FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5012

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RMM/RMM.csproj", "RMM/"]
RUN dotnet restore "RMM/RMM.csproj"
COPY . .
WORKDIR "/src/RMM"
RUN dotnet build "./RMM.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RMM.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=1m CMD curl --fail http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "RMM.dll"]
