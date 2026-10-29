"use client";

import Link from "next/link";
import { useAuth } from "./AuthProvider";

const ROLE_LABEL = { Teacher: "Professor", Student: "Aluno" } as const;

export function Header() {
  const { user, logout } = useAuth();

  async function handleLogout() {
    await logout();
    // Recarrega a página inteira (em vez de router.replace): o Next guarda as páginas visitadas
    // escondidas no DOM, e nada do usuário anterior deve sobrar num computador compartilhado.
    // eslint-disable-next-line @next/next/no-location-assign-relative-destination -- recarga completa intencional
    window.location.assign("/login");
  }

  return (
    <header className="header">
      <div className="container header-inner">
        <Link href="/courses" className="brand">
          <span aria-hidden>🎓</span> Educa+
        </Link>
        {user && (
          <div className="header-user">
            <span>
              {user.name} <span className="muted">· {ROLE_LABEL[user.role]}</span>
            </span>
            <button type="button" className="button button-ghost" onClick={handleLogout}>
              Sair
            </button>
          </div>
        )}
      </div>
    </header>
  );
}
