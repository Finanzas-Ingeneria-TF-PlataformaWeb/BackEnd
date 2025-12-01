# ---------- STAGE 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos el csproj y restauramos dependencias
COPY MiVivienda.Api.csproj ./
RUN dotnet restore MiVivienda.Api.csproj

# Copiamos el resto del código y publicamos
COPY . .
RUN dotnet publish MiVivienda.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- STAGE 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Render por defecto usa el puerto 10000 para los servicios web
# y necesita que el servicio escuche en 0.0.0.0 en ese puerto. :contentReference[oaicite:0]{index=0}
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Copiamos lo publicado en la imagen final
COPY --from=build /app/publish .

# Arrancamos la API
ENTRYPOINT ["dotnet", "MiVivienda.Api.dll"]
