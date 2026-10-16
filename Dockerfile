# ==========================================
# ETAPA 1: Compilación
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copiar los archivos de proyecto
COPY ["ClinicaWeb/ClinicaWeb.csproj", "ClinicaWeb/"]
COPY ["ClinicaData/ClinicaData.csproj", "ClinicaData/"]
COPY ["ClinicaEntidades/ClinicaEntidades.csproj", "ClinicaEntidades/"]

# 2. Restaurar dependencias
RUN dotnet restore "ClinicaWeb/ClinicaWeb.csproj"

# 3. Copiar TODO el código fuente (incluyendo wwwroot y appsettings.json)
COPY . .

# 4. Compilar y publicar
WORKDIR "/src/ClinicaWeb"
RUN dotnet build "ClinicaWeb.csproj" -c Release -o /app/build
RUN dotnet publish "ClinicaWeb.csproj" -c Release -o /app/publish

# ==========================================
# ETAPA 2: Ejecución
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# 5. Puerto dinámico de Railway
ENV ASPNETCORE_URLS=http://*:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ClinicaWeb.dll"]
