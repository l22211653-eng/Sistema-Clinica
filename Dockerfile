# ============================================
# ETAPA 1: Compilación
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar los archivos .csproj
COPY ["ClinicaWeb/ClinicaWeb.csproj", "ClinicaWeb/"]
COPY ["ClinicaData/ClinicaData.csproj", "ClinicaData/"]
COPY ["ClinicaEntidades/ClinicaEntidades.csproj", "ClinicaEntidades/"]

# Restaurar paquetes NuGet
RUN dotnet restore "ClinicaWeb/ClinicaWeb.csproj"

# Copiar TODO el código (incluyendo wwwroot, Views, lib, etc.)
COPY . .

# Compilar y publicar
WORKDIR "/src/ClinicaWeb"
RUN dotnet build "ClinicaWeb.csproj" -c Release -o /app/build
RUN dotnet publish "ClinicaWeb.csproj" -c Release -o /app/publish

# ============================================
# ETAPA 2: Ejecución
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 👇 Copiar TODA la carpeta publicada, incluyendo wwwroot
COPY --from=build /app/publish .

# Verificar que wwwroot exista
RUN ls -la /app/wwwroot || echo "No hay wwwroot"

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ClinicaWeb.dll"]
