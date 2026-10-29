import type { User } from "./types";

/**
 * Sessão do usuário (tokens + dados básicos) guardada no localStorage.
 * Funciona como uma "store externa" para o React (useSyncExternalStore): a leitura é síncrona
 * e todos os componentes são avisados quando a sessão muda, inclusive em outra aba.
 */
export interface Session {
  accessToken: string;
  refreshToken: string;
  user: User;
}

const STORAGE_KEY = "education.session";

// undefined = ainda não lido do localStorage; null = sem sessão.
let cache: Session | null | undefined;
const listeners = new Set<() => void>();

export function getSession(): Session | null {
  if (cache === undefined) {
    try {
      const raw = window.localStorage.getItem(STORAGE_KEY);
      cache = raw ? (JSON.parse(raw) as Session) : null;
    } catch {
      cache = null;
    }
  }
  return cache;
}

export function setSession(session: Session | null): void {
  cache = session;
  try {
    if (session) window.localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    else window.localStorage.removeItem(STORAGE_KEY);
  } catch {
    // localStorage indisponível (ex.: modo privado restrito): a sessão vale só enquanto a página estiver aberta.
  }
  listeners.forEach((notify) => notify());
}

export function subscribeSession(listener: () => void): () => void {
  listeners.add(listener);

  // Login/logout feito em outra aba.
  const onStorage = (event: StorageEvent) => {
    if (event.key !== STORAGE_KEY) return;
    cache = undefined;
    listener();
  };
  window.addEventListener("storage", onStorage);

  return () => {
    listeners.delete(listener);
    window.removeEventListener("storage", onStorage);
  };
}
