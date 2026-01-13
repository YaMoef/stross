FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
RUN apt-get update && apt-get install -y curl ffmpeg && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN curl -L -o /usr/local/bin/yt-dlp "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux" && \
    chmod a+rx /usr/local/bin/yt-dlp

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Stross.Downloader.Soundcloud/Stross.Downloader.Soundcloud.csproj", "Stross.Downloader.Soundcloud/"]
COPY ["Stross.Config/Stross.Config.csproj", "Stross.Config/"]
COPY ["Stross.Proto/Stross.Proto.csproj", "Stross.Proto/"]
RUN dotnet restore "Stross.Downloader.Soundcloud/Stross.Downloader.Soundcloud.csproj"
COPY . .
WORKDIR "/src/Stross.Downloader.Soundcloud"
RUN dotnet build "./Stross.Downloader.Soundcloud.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Stross.Downloader.Soundcloud.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /usr/local/bin/yt-dlp /usr/local/bin/yt-dlp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stross.Downloader.Soundcloud.dll"]
