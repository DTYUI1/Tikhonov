FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WeatherAPI.API/WeatherAPI.API.csproj", "WeatherAPI.API/"]
RUN dotnet restore "WeatherAPI.API/WeatherAPI.API.csproj"
COPY . .
WORKDIR "/src/WeatherAPI.API"
RUN dotnet build "WeatherAPI.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeatherAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "WeatherAPI.API.dll"]