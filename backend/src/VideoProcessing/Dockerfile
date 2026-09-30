# Contexto de build: pasta backend/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Configuração comum a todos os projetos (Nullable, ImplicitUsings, analyzers, warnings como erro).
COPY Directory.Build.props ./
COPY src/VideoProcessing/VideoProcessing.csproj src/VideoProcessing/
RUN dotnet restore src/VideoProcessing/VideoProcessing.csproj

COPY src/VideoProcessing/ src/VideoProcessing/
RUN dotnet publish src/VideoProcessing/VideoProcessing.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

# FFmpeg (inclui ffprobe) roda somente neste serviço.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /videos && chown $APP_UID:$APP_UID /videos
USER $APP_UID

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VideoProcessing.dll"]
