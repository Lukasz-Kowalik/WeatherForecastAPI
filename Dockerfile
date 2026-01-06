FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["WeatherForecastAPI.csproj", "."]
RUN dotnet restore "WeatherForecastAPI.csproj"

COPY . .
RUN dotnet build "WeatherForecastAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeatherForecastAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

RUN mkdir -p /app/data && chown -R $APP_UID /app/data

COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "WeatherForecastAPI.dll"]