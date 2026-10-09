# Plano de Ação — Plataforma de Educação com Streaming de Vídeo

> **Decisões registradas (28/09/2026)** — ver seção 26. Elas prevalecem sobre trechos anteriores deste documento em caso de conflito.

## 1. Contexto do projeto

Projeto baseado no **ODS 4 — Educação de Qualidade**, com foco em oferecer uma plataforma educacional que permita a professores disponibilizarem conteúdos em vídeo para estudantes.

O principal diferencial técnico do projeto será o **processamento e streaming de vídeos utilizando HLS**, com processamento assíncrono através de um microserviço dedicado.

### Objetivo do MVP

Permitir o seguinte fluxo:

```text
Professor
   ↓
Cria curso/aula
   ↓
Faz upload do vídeo
   ↓
Vídeo é armazenado
   ↓
Education API publica evento
   ↓
RabbitMQ
   ↓
Video Processing Worker
   ↓
FFmpeg
   ↓
Geração de HLS
   ↓
Storage
   ↓
Aluno acessa a aula
   ↓
Player reproduz o vídeo
```


---

# 2. Escopo do MVP

## Funcionalidades obrigatórias

### Autenticação

* Cadastro de usuário
* Login
* JWT Access Token
* Refresh Token
* Hash seguro de senha
* Renovação de Access Token
* Logout/revogação do Refresh Token
* Roles:

  * `Teacher`
  * `Student`

### Professores

* Criar curso
* Criar aulas dentro de um curso
* Fazer upload de vídeo
* Visualizar status do processamento do vídeo

### Vídeos

* Upload do vídeo original
* Armazenamento do arquivo original
* Processamento assíncrono
* Conversão através do FFmpeg
* Geração de HLS
* Geração de múltiplas qualidades, inicialmente:

  * 360p
  * 480p
  * 720p
* Geração de `master.m3u8`
* Atualização do status do vídeo

### Alunos

* Visualizar cursos
* Visualizar aulas
* Reproduzir vídeos através de HLS

---

# 3. Fora do escopo do MVP

Não implementar neste primeiro momento:

* Inteligência Artificial
* Chat
* Fórum
* Gamificação
* Certificados
* Aplicativo mobile
* Offline
* Sistema avançado de recomendações
* Analytics avançado
* Kubernetes
* Kafka
* Redis
* Elasticsearch
* API Gateway
* Microserviços separados para usuários, cursos, aulas etc.
* Banco separado para cada microserviço
* CDN
* Object Storage obrigatório em produção

Qualquer funcionalidade que não seja necessária para:

```text
Professor → Upload → Processamento → Streaming → Aluno
```

deve ser considerada fora do MVP.

---

# 4. Stack tecnológica

## Backend

* C#
* ASP.NET Core
* .NET

## Banco de dados

* PostgreSQL

## ORM

* Entity Framework Core
* Provider PostgreSQL/Npgsql

## Mensageria

* RabbitMQ

## Processamento de vídeo

* FFmpeg

## Streaming

* HLS

## Storage

Para o MVP:

* Docker Volume
* Abstração através de `IFileStorage`

Futuramente essa abstração poderá ser implementada utilizando:

* AWS S3
* Azure Blob Storage
* MinIO
* outro Object Storage

## Containerização

* Docker
* Docker Compose

---

# 5. Arquitetura geral

A aplicação terá dois serviços principais.

```text
                         FRONTEND
                            │
                            ▼
                  ┌──────────────────┐
                  │  EDUCATION API   │
                  │                  │
                  │ ASP.NET Core     │
                  │ JWT              │
                  │ Application      │
                  │ Domain           │
                  │ EF Core          │
                  └───────┬──────────┘
                          │
              ┌───────────┼────────────┐
              │           │            │
              ▼           ▼            ▼
         PostgreSQL   Docker Volume   RabbitMQ
                                       │
                                       ▼
                              ┌──────────────────┐
                              │ VIDEO PROCESSING │
                              │                  │
                              │ .NET Worker      │
                              │ Rabbit Consumer  │
                              │ FFmpeg            │
                              └────────┬─────────┘
                                       │
                                       ▼
                                  Docker Volume
                                       │
                                       ▼
                                      HLS
```

---

# 6. Responsabilidade de cada componente

## Education API

Responsável por:

* HTTP
* autenticação
* autorização
* usuários
* cursos
* aulas
* metadados dos vídeos
* upload
* comunicação com PostgreSQL
* comunicação com RabbitMQ
* controle do status do processamento

A API **não deve executar o processamento pesado do vídeo**.

---

## PostgreSQL

Responsável apenas pelos dados da aplicação.

Exemplos:

```text
Users
RefreshTokens
Courses
Lessons
Videos
```

Não armazenar vídeos dentro do PostgreSQL.

---

## Storage

Responsável pelos arquivos.

Exemplo:

```text
/videos
    /original
        /{videoId}
            aula.mp4

    /processed
        /{videoId}
            master.m3u8

            /360p
                playlist.m3u8
                segment001.ts
                segment002.ts

            /480p
                playlist.m3u8
                ...

            /720p
                playlist.m3u8
                ...
```

No MVP, o Storage será implementado utilizando Docker Volume.

---

# 7. RabbitMQ

RabbitMQ será utilizado para comunicação assíncrona entre a API e o Worker.

O RabbitMQ **não transportará os arquivos de vídeo**.

Ele transportará apenas mensagens contendo metadados.

Exemplo:

```json
{
  "videoId": "abc123",
  "originalPath": "original/abc123/aula.mp4"
}
```

Os paths trafegados são **chaves relativas** ao Storage (não caminhos absolutos de disco), para não quebrar a abstração `IFileStorage`.

Evento (fila `video.uploaded`):

```text
VideoUploaded
```

O RabbitMQ é usado **somente** no sentido API → Worker. O retorno do Worker (início, conclusão e falha do processamento) é feito por **chamadas HTTP internas à Education API** (ver seção 26).

---

# 8. Video Processing Worker

O segundo microserviço será um Worker .NET.

Responsabilidades:

1. Consumir mensagens do RabbitMQ
2. Identificar o vídeo
3. Acessar o Storage
4. Localizar o vídeo original
5. Executar o FFmpeg
6. Gerar os arquivos HLS
7. Salvar o resultado no Storage
8. Informar a conclusão à Education API (HTTP interno)
9. Informar falhas de processamento à Education API (HTTP interno)

Estrutura conceitual:

```text
Video Processing
│
├── RabbitMQ Consumer
│
├── VideoProcessor
│
├── IFFmpegService
│
└── IFileStorage
```

---

# 9. FFmpeg

FFmpeg é uma ferramenta de processamento multimídia.

Ele será utilizado pelo Worker para transformar:

```text
aula.mp4
```

em:

```text
HLS
```

incluindo:

```text
master.m3u8
360p
480p
720p
segments
```

O FFmpeg será instalado dentro da imagem Docker do `video-processing`.

A aplicação .NET não deve espalhar comandos FFmpeg pelo código.

Criar uma abstração:

```csharp
public interface IFFmpegService
{
    Task<ProcessingResult> ConvertToHlsAsync(...);
}
```

Assim o `VideoProcessor` conhece apenas a abstração.

---

# 10. Fluxo completo do upload

## Passo 1 — Professor

O professor realiza upload:

```text
aula-fracoes.mp4
```

Frontend:

```text
POST /lessons/{lessonId}/video
```

---

## Passo 2 — Education API

A API:

1. valida autenticação
2. valida autorização
3. recebe o arquivo
4. gera `VideoId`
5. salva o arquivo original no Storage
6. cria registro no PostgreSQL
7. define status:

```text
UPLOADED
```

8. publica `VideoUploaded` no RabbitMQ

> Quem muda o status para `PROCESSING` é o Worker, ao iniciar o processamento.

---

## Passo 3 — RabbitMQ

RabbitMQ recebe:

```json
{
  "videoId": "abc123",
  "originalPath": "original/abc123/aula.mp4"
}
```

---

## Passo 4 — Worker

Worker recebe a mensagem e chama `POST /internal/videos/{id}/processing/start`.
Se a API recusar (vídeo já processado ou em processamento), o Worker descarta a mensagem (idempotência).

Busca:

```text
/videos/original/abc123/aula.mp4
```

---

## Passo 5 — FFmpeg

Worker executa o processamento:

```text
MP4
 ↓
FFmpeg
 ↓
HLS
```

Gerando:

```text
master.m3u8

360p/
480p/
720p/
```

---

## Passo 6 — Storage

Arquivos processados são salvos:

```text
/videos/processed/abc123/
```

---

## Passo 7 — Conclusão

Worker chama:

```text
POST /internal/videos/{id}/processing/complete
```

com informações como:

```json
{
  "streamingPath": "processed/abc123/master.m3u8",
  "durationSeconds": 612.4
}
```

Em caso de erro: `POST /internal/videos/{id}/processing/fail` com `{ "reason": "..." }`.

---

## Passo 8 — Education API

A API recebe a chamada e atualiza:

```text
Video.Status = PROCESSED
Video.StreamingPath = ...
```

---

# 11. Streaming HLS

O aluno não deve baixar o MP4 original inteiro para assistir.

O player recebe:

```text
master.m3u8
```

Esse arquivo referencia as diferentes qualidades.

Exemplo:

```text
master.m3u8
    │
    ├── 360p/playlist.m3u8
    ├── 480p/playlist.m3u8
    └── 720p/playlist.m3u8
```

Cada playlist referencia segmentos:

```text
segment001.ts
segment002.ts
segment003.ts
...
```

O player solicita os segmentos conforme precisa.

Isso permite:

* reprodução progressiva
* múltiplas qualidades
* adaptação à conexão do aluno

---

# 12. Autenticação

A autenticação ficará dentro da `Education API`.

Não criar microserviço separado de autenticação no MVP.

## Cadastro

Usuário informa:

```text
Name
Email
Password
Role
```

A senha nunca será armazenada em texto puro.

Será utilizado password hashing:

```text
Password
   ↓
PasswordHasher
   ↓
PasswordHash
   ↓
PostgreSQL
```

Preferência:

* Argon2id

Alternativamente:

* bcrypt

---

# 13. JWT

Após login:

```text
POST /auth/login
```

A API:

1. busca usuário
2. verifica senha
3. gera Access Token
4. gera Refresh Token

Resposta conceitual:

```json
{
  "accessToken": "...",
  "refreshToken": "..."
}
```

O Access Token terá vida curta.

Exemplo:

```text
15 minutos
```

O tempo exato pode ser definido durante a implementação.

---

# 14. Refresh Token

Refresh Token terá vida mais longa.

Exemplo:

```text
7 dias
```

Os refresh tokens deverão ser armazenados no banco de forma segura.

Tabela:

```text
RefreshTokens

Id
UserId
TokenHash
ExpiresAt
RevokedAt
CreatedAt
```

O token puro não deve ser armazenado.

---

# 15. Refresh Token Rotation

Ao utilizar:

```text
POST /auth/refresh
```

o sistema:

```text
Refresh Token A
      ↓
valida A
      ↓
revoga A
      ↓
gera B
      ↓
novo Access Token
```

Fluxo:

```text
A → B → C → D
```

Isso reduz o risco associado à reutilização de refresh tokens.

---

# 16. Autorização

Utilizar roles:

```text
Teacher
Student
```

Exemplo:

```text
POST /courses
```

somente:

```text
Teacher
```

Pode criar cursos.

Aluno:

```text
GET /courses
```

pode visualizar cursos disponíveis.

Utilizar os mecanismos nativos do ASP.NET Core:

```csharp
[Authorize]
```

e:

```csharp
[Authorize(Roles = "Teacher")]
```

---

# 17. Estrutura da Education API

> **Atualizado em 29/09/2026:** segue o padrão de arquitetura do **LeoFoundation** (ADRs 0001 Clean Architecture e 0002 CQRS com MediatR).

```text
Education.Domain          Common (Entity, AggregateRoot, Result, Error,      → não referencia nada (só MediatR p/ INotification)
                          DomainEvent), Entities (ricas: fábricas, regras,
                          máquina de estados do vídeo), Events,
                          Interfaces (repositórios, IUnitOfWork e portas:
                          IPasswordHasher, ITokenService, IFileStorage, IMessageBus)
Education.Application     MediatR: XxxCommand + Handler + Validator           → Domain
                          (FluentValidation), XxxQuery + Handler,
                          Common/Behaviors (Logging, Validation), DTOs,
                          read repositories (CQRS), AddApplication()
Education.Infrastructure  Persistence (AppDbContext : IUnitOfWork,             → Application, Domain
                          Configurations snake_case, Repositories, Migrations),
                          Authentication (JWT, PasswordHasher), Messaging
                          (RabbitMQ), Storage, AddInfrastructure()
Education.CrossCutting    Authentication (JWT + chave interna), Errors         → Domain, Application, Infrastructure
                          (ApiErrorResponse), Extensions (ToApiResult),
                          Middleware (ExceptionHandlingMiddleware), AddCrossCutting()
Education.API             Endpoints (Minimal API), OpenApi, Program.cs         → Application, Infrastructure, CrossCutting
```

Regras:

* **Interfaces de porta pertencem ao Domain** (repositórios, `IUnitOfWork`, `IFileStorage`, `IMessageBus`...), nunca à Infrastructure: a implementação é que mora lá. Interfaces só de leitura (`ICourseReadRepository`, `ILessonReadRepository`) ficam na Application, como no Foundation.
* **Cada camada registra as próprias dependências** (`AddApplication()`, `AddInfrastructure()`, `AddCrossCutting()`); o `Program.cs` só encadeia.
* **Commands e queries sempre retornam `Result`/`Result<T>`**: erro de negócio não é exceção. Os endpoints convertem com `ToApiResult()`.
* **Regras de negócio ficam nas entidades**:
  * `Video.StartProcessing/Complete/Fail`: máquina de estados;
  * `Course.AddLesson`: só o dono do curso cria aulas;
  * `RefreshToken.Revoke`: detecção de reuso.
* **Eventos de domínio**: `Video.Create` levanta `VideoUploadedEvent`; o `AppDbContext` publica depois do commit, e o `VideoUploadedEventHandler` envia a mensagem para a fila. Se a fila falhar, o vídeo vira `FAILED` (o professor reenvia).
* **Concorrência otimista** (coluna `xmin` do PostgreSQL) em `videos` e `refresh_tokens`: duas entregas da mesma mensagem, ou dois refresh simultâneos com o mesmo token, não passam juntas.
* `Directory.Build.props` em `backend/`: `Nullable`, `ImplicitUsings`, analyzers e **warnings como erro** para todos os projetos.
* `tests/Education.UnitTests`: testes de arquitetura (dependências entre camadas, e Application sem EF/Npgsql/RabbitMQ/ASP.NET), regras do domínio e handlers com NSubstitute.
* Diferença consciente em relação ao Foundation: **leituras com EF Core** (`AsNoTracking`, projeção direto no DTO) em vez de Dapper. A spec pede EF Core como ORM (regra 8) e evita tecnologia extra (regra 1).

---

# 18. Design Patterns e estratégias

Utilizar patterns somente quando houver necessidade real.

## Layered/Clean Architecture

Separar:

```text
API
Application
Domain
Infrastructure
```

---

## CQRS leve

Separar operações de escrita e leitura conceitualmente:

```text
Commands
Queries
```

Exemplos:

```text
CreateCourse
UploadVideo
CreateLesson

GetCourse
GetLesson
GetVideo
```

Não implementar CQRS complexo.

Não utilizar bancos separados.

Não utilizar Event Sourcing.

---

## Dependency Injection

Utilizar DI nativa do ASP.NET Core.

---

## Adapter / Abstração de Storage

Criar:

```csharp
IFileStorage
```

Implementação MVP:

```csharp
LocalFileStorage
```

que utiliza Docker Volume.

Futuramente:

```text
S3FileStorage
AzureBlobStorage
MinioFileStorage
```

poderão implementar a mesma interface.

---

## Facade para FFmpeg

Criar:

```csharp
IFFmpegService
```

para encapsular a complexidade de execução do FFmpeg.

---

## Strategy

O processamento de vídeo poderá utilizar uma abstração:

```csharp
IVideoProcessingStrategy
```

No MVP teremos principalmente:

```text
HlsProcessingStrategy
```

Não criar diversas estratégias sem necessidade.

---

## Event-Driven Architecture

API → Worker através de evento no RabbitMQ:

```text
VideoUploaded
```

Worker → API através de HTTP interno (start / complete / fail), ver seção 26.

---

## Idempotência

O Worker deve ser preparado para lidar com mensagens duplicadas.

Se um vídeo já estiver:

```text
PROCESSING
```

ou:

```text
PROCESSED
```

o Worker não deve simplesmente processá-lo novamente sem verificar o estado.

---

## State Machine simples

Vídeo terá estados:

```text
UPLOADED
   ↓
PROCESSING
   ↓
PROCESSED
```

Com possibilidade de:

```text
PROCESSING
   ↓
FAILED
```

---

# 19. Entidades iniciais

## User

```text
Id
Name
Email
PasswordHash
Role
CreatedAt
```

## RefreshToken

```text
Id
UserId
TokenHash
ExpiresAt
RevokedAt
CreatedAt
```

## Course

```text
Id
Title
Description
TeacherId
CreatedAt
```

## Lesson

```text
Id
CourseId
Title
Description
Order
```

## Video

```text
Id
LessonId
OriginalPath
StreamingPath
Status
Duration
CreatedAt
```

Os campos podem ser ajustados durante a implementação conforme as necessidades reais.

---

# 20. Relacionamentos

```text
User
 │
 │ 1:N
 ▼
Course
 │
 │ 1:N
 ▼
Lesson
 │
 │ 1:1 / 1:N
 ▼
Video
```

Professor:

```text
User (Teacher)
       │
       └── Courses
```

Aluno:

```text
User (Student)
       │
       └── acesso aos Courses
```

O sistema de matrícula/inscrição pode ser mantido simples no MVP e só deve ser adicionado caso seja realmente necessário para o fluxo de demonstração.

---

# 21. Docker Compose

O ambiente deverá ser executável através de Docker Compose.

Serviços esperados:

```text
frontend
education-api
video-processing
postgres
rabbitmq
```

Storage:

```text
Docker Volume
```

Exemplo conceitual:

```text
volumes:
    postgres-data
    video-storage
```

O `video-storage` deverá ser compartilhado entre:

```text
education-api
video-processing
```

para que ambos tenham acesso aos arquivos.

---

# 22. Ordem de implementação

## Dia 1 — Fundação

* Criar solução
* Criar `Education.API`
* Criar `VideoProcessing`
* Configurar Docker
* Configurar PostgreSQL
* Configurar EF Core
* Criar primeira migration
* Configurar RabbitMQ

Objetivo:

```text
API + PostgreSQL + RabbitMQ + Worker
```

funcionando.

---

## Dia 2 — Autenticação

Implementar:

* User
* Password hashing
* Cadastro
* Login
* JWT
* Refresh Token
* Refresh Token rotation
* Roles
* Authorization

Testar tudo através do Swagger.

---

## Dia 3 — Cursos e aulas

Implementar:

* Course
* Lesson
* relacionamentos
* criação
* consulta
* autorização Teacher/Student

---

## Dia 4 — Upload

Implementar:

* Video
* upload
* Docker Volume
* `IFileStorage`
* `LocalFileStorage`
* status do vídeo
* persistência dos metadados

Objetivo:

```text
Professor
   ↓
API
   ↓
Docker Volume
```

funcionando.

---

## Dia 5 — RabbitMQ + Worker

Implementar:

```text
VideoUploaded
```

API:

```text
Upload
 ↓
Storage
 ↓
RabbitMQ
```

Worker:

```text
RabbitMQ
 ↓
Worker
 ↓
Storage
```

---

## Dia 6 — FFmpeg + HLS

Implementar:

```text
MP4
 ↓
FFmpeg
 ↓
360p
480p
720p
 ↓
HLS
```

Gerar:

```text
master.m3u8
```

e playlists/segmentos.

---

## Dia 7 — Streaming + integração

Implementar:

* servir os arquivos HLS
* player no frontend
* consulta do vídeo
* status de processamento
* reprodução do `master.m3u8`
* fluxo completo Professor → Aluno

---

## Dia 8 — Testes e apresentação

Testar:

### Autenticação

```text
Cadastro
Login
JWT
Refresh
Logout
Roles
```

### Cursos

```text
Teacher cria curso
Teacher cria aula
Student consulta
```

### Vídeo

```text
Upload
 ↓
Storage
 ↓
RabbitMQ
 ↓
Worker
 ↓
FFmpeg
 ↓
HLS
 ↓
Student
```

### Falhas

Testar:

* vídeo inválido
* mensagem duplicada
* FFmpeg falhando
* refresh token expirado
* usuário sem permissão

Depois:

* melhorar documentação
* preparar Swagger
* preparar diagrama
* preparar apresentação
* preparar demonstração do fluxo

---

# 23. Critério principal de sucesso

O MVP estará funcional quando conseguirmos demonstrar:

```text
1. Professor faz login
        ↓
2. Professor cria curso
        ↓
3. Professor cria aula
        ↓
4. Professor envia vídeo
        ↓
5. API salva vídeo
        ↓
6. API publica VideoUploaded
        ↓
7. RabbitMQ entrega mensagem
        ↓
8. Worker recebe
        ↓
9. Worker encontra vídeo no Storage
        ↓
10. FFmpeg processa
        ↓
11. HLS é gerado
        ↓
12. Worker informa conclusão
        ↓
13. API atualiza status
        ↓
14. Aluno faz login
        ↓
15. Aluno acessa aula
        ↓
16. Player recebe master.m3u8
        ↓
17. Vídeo é reproduzido
```

Esse é o **fluxo dourado** do projeto.

---

# 24. Regras para a IA que irá auxiliar no desenvolvimento

Ao auxiliar na implementação deste projeto:

1. Não adicionar tecnologias desnecessárias.
2. Não criar microserviços adicionais sem necessidade.
3. Priorizar simplicidade devido ao prazo de 8 dias.
4. Não utilizar IA no produto.
5. Não mover processamento de vídeo para a Education API.
6. Não enviar vídeos pelo RabbitMQ.
7. Não armazenar vídeos no PostgreSQL.
8. Utilizar EF Core como ORM.
9. Utilizar PostgreSQL.
10. Utilizar Docker para o ambiente.
11. Utilizar Docker Volume como Storage do MVP.
12. Manter abstração `IFileStorage`.
13. Utilizar RabbitMQ para comunicação assíncrona.
14. Utilizar FFmpeg para processamento.
15. Utilizar HLS para streaming.
16. Utilizar JWT + Refresh Token.
17. Nunca armazenar senha em texto puro.
18. Utilizar password hashing.
19. Utilizar autorização baseada em roles.
20. Implementar idempotência no processamento de vídeo.
21. Não criar abstrações apenas para seguir padrões de arquitetura.
22. Toda decisão arquitetural deve ter uma justificativa prática.
23. Priorizar código simples, legível e fácil de explicar na apresentação.
24. Antes de implementar uma funcionalidade nova, verificar se ela pertence ao MVP.
25. Não assumir que uma tecnologia é necessária apenas porque é comum em arquiteturas de produção.

---

# 25. Pergunta que deve orientar cada decisão

Antes de adicionar qualquer componente, perguntar:

> **"Isso é necessário para o fluxo Professor → Upload → Processamento → Streaming → Aluno?"**

Se a resposta for não, deixar fora do MVP.

---

# 26. Decisões registradas (28/09/2026)

## 26.1 Status do vídeo

* A API grava `UPLOADED` ao salvar o arquivo original.
* O Worker move para `PROCESSING` ao iniciar, e depois para `PROCESSED` ou `FAILED`.

## 26.2 Retorno do Worker → API (HTTP interno)

O Worker **não acessa o PostgreSQL**. Ele chama endpoints internos da Education API, protegidos por uma chave compartilhada no header `X-Internal-Api-Key` (configurada via variável de ambiente nos dois serviços):

| Método | Rota | Efeito |
|---|---|---|
| POST | `/internal/videos/{id}/processing/start` | `UPLOADED`/`FAILED` → `PROCESSING`. Retorna `409` se já estiver `PROCESSED`, ou `PROCESSING` sem ser reentrega. |
| POST | `/internal/videos/{id}/processing/complete` | `PROCESSING` → `PROCESSED`, grava `StreamingPath` e `Duration`. |
| POST | `/internal/videos/{id}/processing/fail` | `PROCESSING` → `FAILED`, grava `FailureReason`. |

Idempotência:

* `PROCESSED` → Worker descarta a mensagem (ack).
* `PROCESSING` e a mensagem **não** é reentrega → duplicata, descarta.
* `PROCESSING` e a mensagem **é** reentrega (`redelivered = true`) → o Worker anterior caiu no meio; reprocessa sobrescrevendo a saída.
  O Worker envia essa informação no `start` (ex.: `?redelivered=true`).

Com isso, os eventos `VideoProcessed` e `VideoProcessingFailed` **não** trafegam pelo RabbitMQ.

## 26.3 Mensageria

* Biblioteca: `RabbitMQ.Client` 7.x (API assíncrona). Sem MassTransit.
* Fila única: `video.uploaded` (durável, mensagens persistentes).
* Worker: `prefetch = 1`, ack manual. Erro inesperado → `nack` sem requeue (mensagem descartada e vídeo marcado `FAILED` quando possível).
* Contratos (mensagem, chaves do Storage, requests internos) **duplicados** entre a Education (`Application/Videos/Messages`, `Application/Videos/VideoStorageKeys`, `Infrastructure/Messaging/RabbitMqQueues`, `API/Endpoints/InternalVideosEndpoints`) e o Worker (`VideoProcessing/Contracts`), com a mesma estrutura. Os dois serviços não compartilham código; ao mudar um contrato, alterar as duas cópias. O Worker desserializa sem diferenciar maiúsculas/minúsculas para tolerar pequenas divergências.

## 26.4 Storage e paths

* Docker Volume `video-storage` montado em `/videos` na API e no Worker.
* Banco e mensagens guardam **chaves relativas**: `original/{videoId}/original.mp4`, `processed/{videoId}/master.m3u8`.

## 26.5 Streaming HLS

* **Servido por um serviço de mídia dedicado (Nginx, container `media`, porta 8081)**, e não pela Education API. Motivo: cada aluno assistindo pede um segmento a cada ~6 s; esse tráfego não pode disputar recursos com login, cursos e upload.
* O **Worker não serve vídeo**: ele só produz os arquivos. Ele fica com a CPU ocupada pelo FFmpeg e escala pela fila; o streaming escala pelo número de alunos.
* O Nginx monta o volume `video-storage` **somente leitura** e expõe apenas `processed/`. Não serve `original/`, não lista pastas e bloqueia `../`.
* A API devolve a `streamingUrl` absoluta, montada por `IFileStorage.GetPublicUrl` a partir de `Storage:PublicBaseUrl`, ex.: `http://localhost:8081/processed/{videoId}/master.m3u8`. Trocar para S3/Blob + CDN no futuro exige só mudar essa configuração e a implementação do `IFileStorage`.
* MIME types: `.m3u8` → `application/vnd.apple.mpegurl`, `.ts` → `video/mp2t`.
* Cache: playlists 60 s; segmentos 1 dia (cada upload gera um novo `videoId`, então o conteúdo de uma URL não muda).
* **Fase 1 sem autenticação nos arquivos HLS** (o `videoId` é GUID, não adivinhável).
* CORS liberado para a origem do frontend (`FRONTEND_ORIGIN`), com resposta ao preflight.
* Configuração em `media/templates/` (processada pelo `envsubst` da imagem oficial do Nginx).

## 26.6 Upload

* Rota: `POST /v1/lessons/{lessonId}/video` (multipart), somente `Teacher` dono do curso.
* Apenas `.mp4`; limite de **500 MB** (ajustar `MaxRequestBodySize` do Kestrel e `FormOptions.MultipartBodyLengthLimit`).
* Aula → Vídeo é **1:1**. Reenvio substitui o vídeo anterior.

## 26.7 FFmpeg

* H.264 + AAC, segmentos `.ts` de ~6 s, preset `veryfast`.
* Gerar apenas qualidades ≤ resolução original (sem upscale).
* Duração obtida via `ffprobe`.

**Implementado (28/09/2026)** em `VideoProcessing/Processing`:

| Qualidade | Lado menor | Vídeo | Áudio |
|---|---|---|---|
| 360p | 360 | 800 kbps | 96 kbps |
| 480p | 480 | 1400 kbps | 128 kbps |
| 720p | 720 | 2800 kbps | 128 kbps |

* **Um único comando FFmpeg** gera todas as qualidades: decodifica uma vez, divide (`split`) e redimensiona. Keyframes forçados a cada 6 s, então os segmentos ficam alinhados entre as qualidades e o player troca de qualidade sem engasgar.
* "Lado menor" permite vídeo em retrato (ex.: 720x1280 → 360x640, 480x854, 720x1280).
* Vídeo abaixo de 360p gera uma única qualidade na resolução original. Vídeo sem áudio gera HLS só com vídeo.
* O `ffprobe` valida o arquivo antes: conteúdo inválido → `FAILED` com o motivo.
* O processo roda sem shell (`ArgumentList`), com timeout (90 min) e cancelamento. Na falha, as últimas linhas do FFmpeg vão para o `FailureReason`.
* A saída é gerada em `work/{videoId}` e só depois movida para `processed/{videoId}` (rename no mesmo volume). O Nginx nunca enxerga um HLS pela metade.
* O RabbitMQ roda com `consumer_timeout` de 2 h (o padrão de 30 min derrubaria o canal durante conversões longas).
* O padrão Strategy (`IVideoProcessingStrategy`) **não foi criado**: só existe uma estratégia (HLS), e a regra 21 desaconselha a abstração sem necessidade. `IFFmpegService` já isola o FFmpeg.

Fluxo do Worker (`VideoProcessor`):

```text
mensagem → POST start ──409/404──► ignora (ack)
               │ 204
               ▼
        original existe? ──não──► POST fail
               │
        ffprobe + FFmpeg em work/{id} ──erro──► limpa work/ → POST fail
               │ ok
        move para processed/{id} → POST complete → ack
```

Tratamento de falhas no consumer:

* **Worker desligando:** não confirma a mensagem; o RabbitMQ a devolve e ela volta como reentrega, que a API aceita mesmo com o vídeo em `PROCESSING`.
* **API fora do ar:** tenta mais uma vez via reentrega e, na segunda falha, descarta.
* **Chamadas HTTP:** têm retry automático para falhas transitórias (`AddStandardResilienceHandler`).

Cenários testados: 720p com áudio, 480p sem áudio (sem upscale), retrato, arquivo inválido, mensagem duplicada e Worker reiniciado no meio da conversão.

## 26.8 Endpoints do MVP (somente os necessários)

| Método | Rota | Quem |
|---|---|---|
| POST | `/v1/auth/register` | público |
| POST | `/v1/auth/login` | público |
| POST | `/v1/auth/refresh` | público (com refresh token) |
| POST | `/v1/auth/logout` | autenticado |
| GET | `/v1/courses` | autenticado |
| GET | `/v1/courses/{id}` (com aulas) | autenticado |
| POST | `/v1/courses` | Teacher |
| POST | `/v1/courses/{courseId}/lessons` | Teacher dono do curso |
| GET | `/v1/lessons/{id}` (com status e URL do vídeo) | autenticado |
| POST | `/v1/lessons/{lessonId}/video` | Teacher dono do curso |
| GET | `http://localhost:8081/processed/{videoId}/...` (serviço `media`, não é a API) | público (fase 1) |
| POST | `/internal/videos/{id}/processing/*` | Worker (chave interna) |
| GET | `/health` | público |

Autorização por dono do recurso: além da role, verificar `course.TeacherId == userId` nas escritas.

Erros no formato do Foundation, `ApiErrorResponse` `{ statusCode, message, code }` (ex.: `{ 404, "Curso não encontrado.", "Course.NotFound" }`). Erros de negócio vêm do `Result` via `ToApiResult()`: validação (FluentValidation) → **422**, não encontrado → 404, conflito → 409, não autenticado → 401, sem permissão → 403. O `ExceptionHandlingMiddleware` (CrossCutting) cobre as exceções: JSON malformado → 400, corpo acima do limite → 413, concorrência → 409 e qualquer outra → 500 (a mensagem técnica só aparece em Development). O JWT e a chave interna também respondem 401/403 nesse formato. O frontend acompanha o status do vídeo por polling em `GET /v1/lessons/{id}`.

## 26.9 Stack e ambiente

* .NET 10 (LTS), ASP.NET Core com Minimal APIs, MediatR 12.5 (última versão Apache-2.0), FluentValidation 11; OpenAPI nativo + Swagger UI em `/swagger` (Development).
* EF Core 10 + Npgsql; tabelas/colunas em snake_case; migrations aplicadas na inicialização da API em Development (instância única no MVP).
* PostgreSQL 17, RabbitMQ 4 (com painel de gerenciamento).
* FFmpeg instalado apenas na imagem do `video-processing`.
* Postgres exposto no host na porta **5433** (configurável por `POSTGRES_PORT`).

Estrutura do repositório:

```text
/
├── backend/
│   ├── Education.slnx
│   ├── Directory.Build.props
│   ├── tests/Education.UnitTests/
│   └── src/
│       ├── Education.API/
│       ├── Education.Application/
│       ├── Education.CrossCutting/
│       ├── Education.Domain/
│       ├── Education.Infrastructure/
│       └── VideoProcessing/
├── frontend/            (a criar)
├── docker-compose.yml
├── .env.example
└── especificacoes.md
```

## 26.10 Autenticação (implementada)

* **Hash de senha:** `PasswordHasher<T>` nativo do ASP.NET Core Identity (PBKDF2 com salt). Escolhido no lugar do Argon2id para não depender de biblioteca externa. Rehash automático no login quando o formato evoluir.
* **Cadastro:** aberto; o usuário escolhe `Teacher` ou `Student` (limitação conhecida do MVP, citar na apresentação).
* `register` e `login` já retornam o par de tokens.
* Access Token: 15 min, HMAC-SHA256, claims `sub`, `email`, `name`, `role`.
* Refresh Token: 7 dias, 64 bytes aleatórios; banco guarda só o SHA-256.
* Rotação atômica. **Reuso** de um refresh token já rotacionado revoga todos os refresh tokens ativos do usuário.
* Chave do JWT e chave interna via configuração (`Jwt__SigningKey`, `InternalApi__Key`); a API não sobe sem elas.
* Arquivo original salvo como `original/{videoId}/original.mp4`. Reenvio gera novo `videoId` e apaga os arquivos antigos.

## 26.11 Ainda em aberto

* **Frontend:** provavelmente Next.js; player com `hls.js`.
* **Refresh token no cliente:** `localStorage` ou cookie `httpOnly` (a API hoje devolve no corpo da resposta).
