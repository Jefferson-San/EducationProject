# Contexto de build: pasta backend/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Configuração comum a todos os projetos (Nullable, ImplicitUsings, analyzers, warnings como erro).
COPY Directory.Build.props ./

# Primeiro só os .csproj, para o restore ficar em cache enquanto o código muda.
COPY src/Education.Domain/Education.Domain.csproj src/Education.Domain/
COPY src/Education.Application/Education.Application.csproj src/Education.Application/
COPY src/Education.Infrastructure/Education.Infrastructure.csproj src/Education.Infrastructure/
COPY src/Education.CrossCutting/Education.CrossCutting.csproj src/Education.CrossCutting/
COPY src/Education.API/Education.API.csproj src/Education.API/
RUN dotnet restore src/Education.API/Education.API.csproj

COPY src/Education.Domain/ src/Education.Domain/
COPY src/Education.Application/ src/Education.Application/
COPY src/Education.Infrastructure/ src/Education.Infrastructure/
COPY src/Education.CrossCutting/ src/Education.CrossCutting/
COPY src/Education.API/ src/Education.API/
RUN dotnet publish src/Education.API/Education.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Diretório do volume compartilhado; criado aqui para que o volume herde o dono (usuário não-root "app").
RUN mkdir -p /videos && chown $APP_UID:$APP_UID /videos
USER $APP_UID

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Education.API.dll"]
