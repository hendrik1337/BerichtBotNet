FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BerichtBotNet.csproj", "BerichtBotNet/"]
RUN dotnet restore "BerichtBotNet.csproj"
COPY . .
WORKDIR "/src/BerichtBotNet"
RUN dotnet build "BerichtBotNet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BerichtBotNet.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BerichtBotNet.dll"]
