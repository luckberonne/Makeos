#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# El paquete NuGet de Tesseract necesita las librerías nativas libtesseract y libleptonica.
# En Linux busca los nombres "libleptonica-1.82.0.so" y "libtesseract50.so", por lo que se
# crean symlinks hacia las versiones que provee Debian.
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends libleptonica-dev libtesseract-dev \
    && rm -rf /var/lib/apt/lists/* \
    && ln -sf /usr/lib/x86_64-linux-gnu/liblept.so.5 /usr/lib/x86_64-linux-gnu/libleptonica-1.82.0.so \
    && ln -sf /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /usr/lib/x86_64-linux-gnu/libtesseract50.so

USER app
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
ENTRYPOINT ["dotnet", "Makeos.dll"]
