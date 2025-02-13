FROM mcr.microsoft.com/dotnet/aspnet AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ParserConsole/ParserConsole.csproj", "ParserConsole/"]
RUN dotnet restore "ParserConsole/ParserConsole.csproj"
COPY . .
WORKDIR "/src/ParserConsole"
RUN dotnet build "ParserConsole.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ParserConsole.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM base AS final
EXPOSE 8090
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ParserConsole.dll"]
