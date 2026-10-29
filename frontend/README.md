# Educa+ — frontend

Next.js 16 (App Router) + TypeScript + hls.js. Consome a Education API e o serviço de mídia (HLS).

```bash
npm install
npm run dev
```

Abre em http://localhost:3000. A API precisa estar no ar (`docker compose up -d` na raiz).
`NEXT_PUBLIC_API_URL` (padrão `http://localhost:8080`) define onde o navegador encontra a API.

| Rota | Tela |
|---|---|
| `/login` | Entrar / criar conta (Professor ou Aluno) |
| `/courses` | Cursos; o professor cria curso |
| `/courses/[id]` | Aulas e status de cada vídeo; o dono cria aula |
| `/lessons/[id]` | Player HLS, status ao vivo (polling) e upload do vídeo (dono) |
