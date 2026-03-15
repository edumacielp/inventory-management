FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/InventoryManagement.API/InventoryManagement.API.csproj", "src/InventoryManagement.API/"]
COPY ["src/InventoryManagement.Application/InventoryManagement.Application.csproj", "src/InventoryManagement.Application/"]
COPY ["src/InventoryManagement.Domain/InventoryManagement.Domain.csproj", "src/InventoryManagement.Domain/"]
COPY ["src/InventoryManagement.Infrastructure/InventoryManagement.Infrastructure.csproj", "src/InventoryManagement.Infrastructure/"]

RUN dotnet restore "./src/InventoryManagement.API/InventoryManagement.API.csproj"

COPY . .
WORKDIR "/src/src/InventoryManagement.API"

RUN dotnet build "./InventoryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InventoryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "InventoryManagement.API.dll"]