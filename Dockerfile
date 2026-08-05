FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/UrlShortener.Api/UrlShortener.Api.csproj", "src/UrlShortener.Api/"]
RUN dotnet restore "src/UrlShortener.Api/UrlShortener.Api.csproj"
COPY src/UrlShortener.Api/ src/UrlShortener.Api/
RUN dotnet publish "src/UrlShortener.Api/UrlShortener.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
CMD ["sh", "-c", "dotnet UrlShortener.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]