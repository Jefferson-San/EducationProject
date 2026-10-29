import { API_URL } from "./config";
import { getSession, setSession } from "./session";
import type { ApiErrorResponse, AuthResponse } from "./types";

/** Erro devolvido pela API, já com a mensagem pronta para mostrar ao usuário. */
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
    public readonly code?: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

async function toApiError(response: Response): Promise<ApiError> {
  try {
    const body = (await response.json()) as Partial<ApiErrorResponse>;
    if (body?.message) return new ApiError(response.status, body.message, body.code);
  } catch {
    // corpo vazio ou não-JSON
  }
  return new ApiError(response.status, `Erro ${response.status} ao falar com a API.`);
}

// ─────────────────────────── refresh token ───────────────────────────

let refreshing: Promise<boolean> | null = null;

/**
 * Troca o refresh token por um novo par. Várias requisições recebendo 401 ao mesmo tempo
 * compartilham uma única chamada: o backend rotaciona o token e trataria a segunda como reuso.
 */
export function refreshSession(): Promise<boolean> {
  refreshing ??= (async () => {
    const session = getSession();
    if (!session) return false;
    try {
      const response = await fetch(`${API_URL}/v1/auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken: session.refreshToken }),
      });
      if (!response.ok) {
        setSession(null); // refresh expirado, revogado ou reutilizado: precisa logar de novo
        return false;
      }
      const auth = (await response.json()) as AuthResponse;
      setSession({ accessToken: auth.accessToken, refreshToken: auth.refreshToken, user: auth.user });
      return true;
    } catch {
      return false; // API fora do ar: mantém a sessão e deixa a requisição original falhar
    } finally {
      refreshing = null;
    }
  })();
  return refreshing;
}

// ─────────────────────────── requisições ───────────────────────────

interface RequestOptions {
  method?: string;
  body?: unknown;
  /** Envia o access token (padrão: true). */
  auth?: boolean;
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}, retried = false): Promise<T> {
  const { method = "GET", body, auth = true } = options;
  const headers: Record<string, string> = {};
  if (body !== undefined) headers["Content-Type"] = "application/json";

  const session = auth ? getSession() : null;
  if (session) headers.Authorization = `Bearer ${session.accessToken}`;

  let response: Response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch {
    throw new ApiError(0, "Não foi possível conectar à API. Verifique se o backend está rodando.");
  }

  // Access token expirado (15 min): renova uma vez e repete a requisição.
  if (response.status === 401 && session && !retried && (await refreshSession())) {
    return apiRequest<T>(path, options, true);
  }

  if (!response.ok) throw await toApiError(response);
  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}

export const api = {
  get: <T>(path: string) => apiRequest<T>(path),
  post: <T>(path: string, body?: unknown, auth = true) => apiRequest<T>(path, { method: "POST", body, auth }),
};

// ─────────────────────────── upload com progresso ───────────────────────────

/**
 * Upload multipart do vídeo. Usa XMLHttpRequest porque o fetch não informa o progresso do envio.
 * @param onProgress percentual de 0 a 100
 */
export function uploadFile<T>(path: string, file: File, onProgress: (percent: number) => void, retried = false): Promise<T> {
  return new Promise<T>((resolve, reject) => {
    const session = getSession();
    const xhr = new XMLHttpRequest();
    xhr.open("POST", `${API_URL}${path}`);
    if (session) xhr.setRequestHeader("Authorization", `Bearer ${session.accessToken}`);

    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable) onProgress(Math.round((event.loaded / event.total) * 100));
    };

    xhr.onload = async () => {
      if (xhr.status === 401 && session && !retried && (await refreshSession())) {
        uploadFile<T>(path, file, onProgress, true).then(resolve, reject);
        return;
      }
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve((xhr.responseText ? JSON.parse(xhr.responseText) : undefined) as T);
        return;
      }
      try {
        const body = JSON.parse(xhr.responseText) as ApiErrorResponse;
        reject(new ApiError(xhr.status, body.message, body.code));
      } catch {
        reject(new ApiError(xhr.status, `Erro ${xhr.status} no envio do vídeo.`));
      }
    };
    xhr.onerror = () => reject(new ApiError(0, "Falha de conexão durante o envio do vídeo."));

    const form = new FormData();
    form.append("file", file);
    xhr.send(form);
  });
}
