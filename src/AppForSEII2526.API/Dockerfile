# 1. IMAGEN DE CONSTRUCCIÓN
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos el csproj (Asumo que dentro de esa carpeta se llama AppForSEII2526.API.csproj)
COPY ["AppForSEII2526.API.csproj", "./"]
RUN dotnet restore "AppForSEII2526.API.csproj"

# Copiar todo el código de la API
COPY . .
RUN dotnet publish -c Release -o /app/out

# 2. IMAGEN FINAL (ASP.NET)
# Aquí usamos 'aspnet' porque es una Web API
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Configuración de puerto y entorno
ENV ASPNETCORE_URLS=http://+:80

# IMPORTANTE: El nombre del DLL generado
ENTRYPOINT ["dotnet", "AppForSEII2526.API.dll"]