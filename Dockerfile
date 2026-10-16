# ==========================================
# ETAPA 1: Compilación
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar los archivos de proyecto
COPY ["ClinicaWeb/ClinicaWeb.csproj", "ClinicaWeb/"]
COPY ["ClinicaData/ClinicaData.csproj", "ClinicaData/"]
COPY ["ClinicaEntidades/ClinicaEntidades.csproj", "ClinicaEntidades/"]

# Restaurar dependencias
RUN dotnet restore "ClinicaWeb/ClinicaWeb.csproj"

# Copiar TODO el código fuente
COPY . .

# Compilar y publicar
WORKDIR "/src/ClinicaWeb"
RUN dotnet build "ClinicaWeb.csproj" -c Release -o /app/build
RUN dotnet publish "ClinicaWeb.csproj" -c Release -o /app/publish

# ==========================================
# ETAPA 2: Ejecución
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# ⚠️ ESTO ES LO NUEVO: Instalar la librería que necesita Npgsql para Kerberos
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Configurar el puerto dinámico de Railway
ENV ASPNETCORE_URLS=http://*:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ClinicaWeb.dll"]
