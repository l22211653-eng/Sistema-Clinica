# ============================================
# ETAPA 1: Compilación
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ClinicaWeb/ClinicaWeb.csproj", "ClinicaWeb/"]
COPY ["ClinicaData/ClinicaData.csproj", "ClinicaData/"]
COPY ["ClinicaEntidades/ClinicaEntidades.csproj", "ClinicaEntidades/"]

RUN dotnet restore "ClinicaWeb/ClinicaWeb.csproj"

COPY . .

WORKDIR "/src/ClinicaWeb"
RUN dotnet build "ClinicaWeb.csproj" -c Release -o /app/build
RUN dotnet publish "ClinicaWeb.csproj" -c Release -o /app/publish

# ============================================
# ETAPA 2: Ejecución
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# 👇 ESTA LÍNEA ES LA CLAVE
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENTRYPOINT ["dotnet", "ClinicaWeb.dll"]
