# Plataforma de Educação com Streaming de Vídeo (ODS 4)

Professores publicam aulas em vídeo; um worker converte os vídeos para HLS (360p/480p/720p) e os alunos assistem via streaming.

Especificação completa: [especificacoes.md](especificacoes.md).

## Serviços

| Serviço | Tecnologia | Porta no host |
|---|---|---|
| `education-api` | ASP.NET Core (.NET 10) | http://localhost:8080 (Swagger em `/swagger`) |
| `video-processing` | .NET Worker + FFmpeg (converte .mp4 em HLS 360p/480p/720p) | — |
| `media` | Nginx (serve o HLS do volume, somente leitura) | http://localhost:8081 |
| `postgres` | PostgreSQL 17 | 5433 |
| `rabbitmq` | RabbitMQ 4 | 5672 (painel: http://localhost:15672, guest/guest) |

## Rodando com Docker

```bash
docker compose up -d --build
```

Verificar:

```bash
curl http://localhost:8080/health
```

Logs do worker:

```bash
docker compose logs -f video-processing
```

As migrations do banco são aplicadas automaticamente quando a API inicia.

## Collection do Postman

[postman/Education.postman_collection.json](postman/Education.postman_collection.json) documenta todos os endpoints com exemplos de payload e resposta. Importe no Postman (File → Import) e execute as pastas em ordem: os tokens e ids são preenchidos automaticamente.

## Testando pelo Swagger

1. `POST /v1/auth/register` com `role` = `Teacher` ou `Student` (já retorna os tokens).
2. Copie o `accessToken`, clique em **Authorize** e cole o token.
3. Como Teacher: `POST /v1/courses` → `POST /v1/courses/{courseId}/lessons` → `POST /v1/lessons/{lessonId}/video` (.mp4, até 500 MB).
4. `GET /v1/lessons/{id}` mostra o status do vídeo (`Uploaded` → `Processing` → `Processed`/`Failed`) e, quando pronto, a `streamingUrl`, que aponta para o serviço `media` (Nginx), não para a API.

Os endpoints `/internal/videos/...` são usados só pelo Worker (header `X-Internal-Api-Key`) e não aparecem no Swagger.

## Testes

```bash
dotnet test backend/Education.slnx
```

Inclui testes de arquitetura: uma referência proibida entre camadas quebra o build de testes.

## Rodando localmente (fora do Docker)

Suba só a infraestrutura e rode os projetos .NET na máquina:

```bash
docker compose up -d postgres rabbitmq
```

```bash
dotnet run --project backend/src/Education.API
```

```bash
dotnet run --project backend/src/VideoProcessing
```

## Migrations

```bash
dotnet ef migrations add NomeDaMigration --project backend/src/Education.Infrastructure --startup-project backend/src/Education.API --output-dir Persistence/Migrations
```

> Ao rodar a API fora do Docker, os uploads vão para a pasta local `storage/`, mas o serviço `media` lê o volume do Docker. Para testar o streaming de ponta a ponta, use tudo via `docker compose`.
