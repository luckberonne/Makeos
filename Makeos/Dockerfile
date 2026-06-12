#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# El paquete NuGet de Tesseract usa un cargador nativo propio que NO consulta /usr/lib:
# busca las librerías junto al ejecutable y en el subdirectorio ./x64, y con los nombres
# "libleptonica-1.82.0.so" y "libtesseract50.so". Además invoca dlopen a través de
# "libdl.so", que en glibc moderno (Debian 12) ya viene fusionado en libc y solo expone
# "libdl.so.2". Por eso, además de instalar las librerías nativas, se crean los symlinks
# con los nombres esperados y el de libdl.
USER root
RUN set -eux; \
    apt-get update; \
    apt-get install -y --no-install-recommends libleptonica-dev libtesseract-dev curl; \
    rm -rf /var/lib/apt/lists/*; \
    ARCH_DIR="/usr/lib/$(uname -m)-linux-gnu"; \
    ln -sf "$ARCH_DIR/libdl.so.2" "$ARCH_DIR/libdl.so"; \
    ln -sf "$(ls "$ARCH_DIR"/liblept*.so.* | head -1)" "$ARCH_DIR/libleptonica-1.82.0.so"; \
    ln -sf "$(ls "$ARCH_DIR"/libtesseract.so.* | head -1)" "$ARCH_DIR/libtesseract50.so"

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Makeos/Makeos.csproj", "Makeos/"]
RUN dotnet restore "./Makeos/Makeos.csproj"
COPY . .
WORKDIR "/src/Makeos"
RUN dotnet build "./Makeos.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Makeos.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# El cargador nativo de Tesseract busca las librerías en ./x64 junto al ejecutable.
RUN set -eux; \
    ARCH_DIR="/usr/lib/$(uname -m)-linux-gnu"; \
    mkdir -p /app/x64; \
    ln -sf "$ARCH_DIR/libleptonica-1.82.0.so" /app/x64/libleptonica-1.82.0.so; \
    ln -sf "$ARCH_DIR/libtesseract50.so" /app/x64/libtesseract50.so; \
    chown -R app:app /app/x64

USER app

# Comprobación de salud del contenedor contra el endpoint /health.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Makeos.dll"]
