FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Warehouse.API/Warehouse.API.csproj", "Warehouse.API/"]
COPY ["Warehouse.Application/Warehouse.Application.csproj", "Warehouse.Application/"]
COPY ["Warehouse.Infrastructure/Warehouse.Infrastructure.csproj", "Warehouse.Infrastructure/"]
COPY ["Warehouse.Domain/Warehouse.Domain.csproj", "Warehouse.Domain/"]

RUN dotnet restore "Warehouse.API/Warehouse.API.csproj"

COPY . .
RUN dotnet publish "Warehouse.API/Warehouse.API.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Warehouse.API.dll"]
