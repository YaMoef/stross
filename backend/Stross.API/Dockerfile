FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Stross.API/Stross.API.csproj", "Stross.API/"]
COPY ["Stross.Application/Stross.Application.csproj", "Stross.Application/"]
COPY ["Stross.Domain/Stross.Domain.csproj", "Stross.Domain/"]
COPY ["Stross.Exception/Stross.Exception.csproj", "Stross.Exception/"]
COPY ["Stross.Infrastructure/Stross.Infrastructure.csproj", "Stross.Infrastructure/"]
COPY ["Stross.Config/Stross.Config.csproj", "Stross.Config/"]
COPY ["Stross.Proto/Stross.Proto.csproj", "Stross.Proto/"]
COPY ["Stross.SubsonicModels/Stross.SubsonicModels.csproj", "Stross.SubsonicModels/"]
RUN dotnet restore "Stross.API/Stross.API.csproj"
COPY . .
WORKDIR "/src/Stross.API"
RUN dotnet build "./Stross.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Stross.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stross.API.dll"]
