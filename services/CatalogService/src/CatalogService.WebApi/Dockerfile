FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_USER
ARG GITHUB_TOKEN
ENV GITHUB_USER=$GITHUB_USER
ENV GITHUB_TOKEN=$GITHUB_TOKEN
WORKDIR /src

COPY ["nuget.config", "."]

COPY ["services/CatalogService/src/CatalogService.WebApi/CatalogService.WebApi.csproj", "services/CatalogService/src/CatalogService.WebApi/"]
COPY ["services/CatalogService/src/CatalogService.Business/CatalogService.Business.csproj", "services/CatalogService/src/CatalogService.Business/"]
COPY ["services/CatalogService/src/CatalogService.Repository/CatalogService.Repository.csproj", "services/CatalogService/src/CatalogService.Repository/"]

RUN dotnet restore "services/CatalogService/src/CatalogService.WebApi/CatalogService.WebApi.csproj"

COPY . .
WORKDIR "/src/services/CatalogService/src/CatalogService.WebApi"
RUN dotnet build "./CatalogService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CatalogService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.WebApi.dll"]