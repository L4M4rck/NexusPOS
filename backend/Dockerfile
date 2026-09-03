FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["NuGet.Config", "."]
COPY ["backend/src/NexusPOS.Domain/NexusPOS.Domain.csproj", "backend/src/NexusPOS.Domain/"]
COPY ["backend/src/NexusPOS.Application/NexusPOS.Application.csproj", "backend/src/NexusPOS.Application/"]
COPY ["backend/src/NexusPOS.Infrastructure/NexusPOS.Infrastructure.csproj", "backend/src/NexusPOS.Infrastructure/"]
COPY ["backend/src/NexusPOS.Api/NexusPOS.Api.csproj", "backend/src/NexusPOS.Api/"]
RUN dotnet restore backend/src/NexusPOS.Api/NexusPOS.Api.csproj --configfile NuGet.Config
COPY backend/src backend/src
RUN dotnet publish backend/src/NexusPOS.Api/NexusPOS.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "NexusPOS.Api.dll"]
