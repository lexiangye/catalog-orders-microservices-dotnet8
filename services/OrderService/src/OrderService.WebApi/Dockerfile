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

COPY ["services/OrderService/src/OrderService.WebApi/OrderService.WebApi.csproj", "services/OrderService/src/OrderService.WebApi/"]
COPY ["services/OrderService/src/OrderService.Business/OrderService.Business.csproj", "services/OrderService/src/OrderService.Business/"]
COPY ["services/OrderService/src/OrderService.Repository/OrderService.Repository.csproj", "services/OrderService/src/OrderService.Repository/"]

RUN dotnet restore "services/OrderService/src/OrderService.WebApi/OrderService.WebApi.csproj"

COPY . .
WORKDIR "/src/services/OrderService/src/OrderService.WebApi"
RUN dotnet build "./OrderService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OrderService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.WebApi.dll"]