FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MovieBot.Api/MovieBot.Api.csproj", "src/MovieBot.Api/"]
COPY ["src/MovieBot.Data/MovieBot.Data.csproj", "src/MovieBot.Data/"]
COPY ["src/MovieBot.Domain/MovieBot.Domain.csproj", "src/MovieBot.Domain/"]
COPY ["src/MovieBot.Service/MovieBot.Service.csproj", "src/MovieBot.Service/"]
RUN dotnet restore "src/MovieBot.Api/MovieBot.Api.csproj"
COPY . .
WORKDIR "/src/src/MovieBot.Api"
RUN dotnet build "MovieBot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MovieBot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MovieBot.Api.dll"]
