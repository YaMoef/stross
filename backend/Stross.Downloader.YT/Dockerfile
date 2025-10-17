FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Downloader/Downloader.csproj", "Downloader/"]
COPY ["Config/Config.csproj", "Config/"]
COPY ["Proto/Proto.csproj", "Proto/"]
RUN dotnet restore "Downloader/Downloader.csproj"
COPY . .
WORKDIR "/src/Downloader"
RUN dotnet build "Downloader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Downloader.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Downloader.dll"]
