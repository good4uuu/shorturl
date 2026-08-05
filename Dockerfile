FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/UrlShortener.Api/UrlShortener.Api.csproj", "src/UrlShortener.Api/"]
COPY ["src/UrlShortener.Application/UrlShortener.Application.csproj", "src/UrlShortener.Application/"]
COPY ["src/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj", "src/UrlShortener.Infrastructure/"]
RUN dotnet restore "src/UrlShortener.Api/UrlShortener.Api.csproj"
COPY src/UrlShortener.Application/ src/UrlShortener.Application/
COPY src/UrlShortener.Infrastructure/ src/UrlShortener.Infrastructure/
COPY src/UrlShortener.Api/ src/UrlShortener.Api/
RUN dotnet publish "src/UrlShortener.Api/UrlShortener.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
COPY --from=build /app/publish .
EXPOSE 10000
CMD ["sh", "-c", "dotnet UrlShortener.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]