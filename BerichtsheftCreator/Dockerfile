FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY *.csproj .
COPY . .

RUN dotnet restore

RUN dotnet build -c Release --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c Release -o /app/publish --no-restore 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BerichtsheftCreator.dll"]
