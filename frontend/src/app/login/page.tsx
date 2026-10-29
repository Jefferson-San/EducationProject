"use client";

import { useEffect, useLayoutEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";
import { ApiError } from "@/lib/api";
import type { UserRole } from "@/lib/types";

type Mode = "login" | "register";

export default function LoginPage() {
  const { status, login, register } = useAuth();
  const router = useRouter();

  const [mode, setMode] = useState<Mode>("login");
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState<UserRole>("Student");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (status === "authenticated") router.replace("/courses");
  }, [status, router]);

  // Com cacheComponents o Next esconde (não desmonta) as páginas visitadas, preservando o estado.
  // Para a tela de login isso é ruim: senha e perfil do usuário anterior ficariam preenchidos.
  // A limpeza roda quando a página é escondida.
  useLayoutEffect(() => {
    return () => {
      setMode("login");
      setName("");
      setEmail("");
      setPassword("");
      setRole("Student");
      setError(null);
    };
  }, []);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      if (mode === "login") await login(email, password);
      else await register(name, email, password, role);
      router.replace("/courses");
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Erro inesperado.");
    } finally {
      setSubmitting(false);
    }
  }

  function switchMode(next: Mode) {
    setMode(next);
    setError(null);
  }

  return (
    <div className="auth">
      <div className="card">
        <h1 className="auth-title">{mode === "login" ? "Entrar" : "Criar conta"}</h1>
        <p className="muted auth-subtitle">Aulas em vídeo que se adaptam à sua conexão.</p>

        <div className="tabs" role="tablist">
          <button type="button" role="tab" aria-selected={mode === "login"} className={mode === "login" ? "tab tab-active" : "tab"} onClick={() => switchMode("login")}>
            Entrar
          </button>
          <button type="button" role="tab" aria-selected={mode === "register"} className={mode === "register" ? "tab tab-active" : "tab"} onClick={() => switchMode("register")}>
            Criar conta
          </button>
        </div>

        <form onSubmit={handleSubmit} className="form">
          {mode === "register" && (
            <label className="field">
              Nome
              <input value={name} onChange={(e) => setName(e.target.value)} required minLength={2} maxLength={150} autoComplete="name" />
            </label>
          )}
          <label className="field">
            E-mail
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoComplete="email" />
          </label>
          <label className="field">
            Senha
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={mode === "register" ? 8 : undefined}
              autoComplete={mode === "login" ? "current-password" : "new-password"}
            />
            {mode === "register" && <span className="hint">Mínimo de 8 caracteres.</span>}
          </label>

          {mode === "register" && (
            <fieldset className="field">
              <legend>Eu sou</legend>
              <div className="radio-group">
                <label className="radio">
                  <input type="radio" name="role" checked={role === "Student"} onChange={() => setRole("Student")} /> Aluno
                </label>
                <label className="radio">
                  <input type="radio" name="role" checked={role === "Teacher"} onChange={() => setRole("Teacher")} /> Professor
                </label>
              </div>
            </fieldset>
          )}

          {error && <p className="alert alert-error">{error}</p>}

          <button type="submit" className="button" disabled={submitting}>
            {submitting ? "Aguarde…" : mode === "login" ? "Entrar" : "Criar conta"}
          </button>
        </form>
      </div>
    </div>
  );
}
