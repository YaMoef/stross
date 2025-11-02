FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Stross.Downloader.YT/Stross.Downloader.YT.csproj", "Stross.Downloader.YT/"]
COPY ["Stross.Config/Stross.Config.csproj", "Stross.Config/"]
COPY ["Stross.Proto/Stross.Proto.csproj", "Stross.Proto/"]
RUN dotnet restore "Stross.Downloader.YT/Stross.Downloader.YT.csproj"
COPY . .
WORKDIR "/src/Stross.Downloader.YT"
RUN dotnet build "./Stross.Downloader.YT.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Stross.Downloader.YT.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN https://github.com/yt-dlp/yt-dlp/releases/download/2025.10.22/yt-dlp -O /usr/local/bin/yt-dlp && \
    chmod a+rx /usr/local/bin/yt-dlp

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stross.Downloader.YT.dll"]
