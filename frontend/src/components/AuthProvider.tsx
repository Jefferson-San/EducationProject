"use client";

import { createContext, useCallback, useContext, useMemo, useSyncExternalStore } from "react";
import { api } from "@/lib/api";
import { getSession, setSession, subscribeSession, type Session } from "@/lib/session";
import type { AuthResponse, User, UserRole } from "@/lib/types";

type AuthStatus = "loading" | "authenticated" | "anonymous";

interface AuthContextValue {
  status: AuthStatus;
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  register: (name: string, email: string, password: string, role: UserRole) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

// No servidor e durante a hidratação não há localStorage: "undefined" = ainda carregando.
const getServerSession = () => undefined;

function saveSession(auth: AuthResponse) {
  setSession({ accessToken: auth.accessToken, refreshToken: auth.refreshToken, user: auth.user });
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const session: Session | null | undefined = useSyncExternalStore(subscribeSession, getSession, getServerSession);

  const login = useCallback(async (email: string, password: string) => {
    saveSession(await api.post<AuthResponse>("/v1/auth/login", { email, password }, false));
  }, []);

  const register = useCallback(async (name: string, email: string, password: string, role: UserRole) => {
    saveSession(await api.post<AuthResponse>("/v1/auth/register", { name, email, password, role }, false));
  }, []);

  const logout = useCallback(async () => {
    const current = getSession();
    try {
      // Revoga o refresh token no backend; o access token expira sozinho em até 15 min.
      if (current) await api.post("/v1/auth/logout", { refreshToken: current.refreshToken });
    } catch {
      // Mesmo que a API falhe, a sessão local é encerrada.
    } finally {
      setSession(null);
    }
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    status: session === undefined ? "loading" : session ? "authenticated" : "anonymous",
    user: session?.user ?? null,
    login,
    register,
    logout,
  }), [session, login, register, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth deve ser usado dentro de <AuthProvider>.");
  return context;
}
