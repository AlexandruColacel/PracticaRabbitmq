# 1. IMAGEN DE CONSTRUCCIÓN
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# COPIAR EL CSPROJ EXACTO QUE VEO EN TU FOTO
COPY ["LogViewer.csproj", "./"]
RUN dotnet restore "LogViewer.csproj"

# Copiar el resto de archivos (Program.cs, LogEntry.cs, Suscriber.cs)
COPY . .
RUN dotnet publish -c Release -o /app/out

# 2. IMAGEN FINAL (RUNTIME)
# Usamos 'runtime' porque es una app de consola (más ligera)
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/out .

# IMPORTANTE: El nombre del DLL será igual al nombre del .csproj
ENTRYPOINT ["dotnet", "LogViewer.dll"]  