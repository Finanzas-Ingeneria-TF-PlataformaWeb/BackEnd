# 1. ETAPA DE CONSTRUCCIÓN (BUILD STAGE)
# Usa la imagen del SDK (Software Development Kit) para compilar la aplicación.
# Asumo que estás usando .NET 8.0, basado en el error que mencionaste.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el archivo de la solución y los archivos de proyecto (.csproj) para restaurar los paquetes NuGet.
# Esto aprovecha el cache de Docker si los archivos de proyecto no cambian.
COPY *.sln .
COPY MiVivienda.Api/*.csproj MiVivienda.Api/
COPY MiVivienda.Domain/*.csproj MiVivienda.Domain/
# Si tienes otros proyectos (librerías), asegúrate de listarlos también arriba.

RUN dotnet restore MiVivienda.Api/MiVivienda.Api.csproj

# Copia todo el código fuente.
COPY . .

# Cambia el directorio de trabajo al proyecto de API.
WORKDIR /src/MiVivienda.Api

# Publica la aplicación, configurando la salida en una carpeta 'out'.
# --no-restore evita una restauración redundante.
RUN dotnet publish "MiVivienda.Api.csproj" -c Release -o /app/out --no-restore


# 2. ETAPA FINAL (FINAL STAGE)
# Usa la imagen del Runtime (solo lo necesario para ejecutar) para el despliegue final.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia los archivos publicados desde la etapa de 'build'.
COPY --from=build /app/out .

# Define el punto de entrada de la aplicación.
# Reemplaza 'MiVivienda.Api.dll' con el nombre de tu archivo .dll principal si es diferente.
ENTRYPOINT ["dotnet", "MiVivienda.Api.dll"]
