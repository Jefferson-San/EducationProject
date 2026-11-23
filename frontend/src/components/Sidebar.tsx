"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "./AuthProvider";

/** Itens do menu. `match` decide quando o link fica destacado como ativo. */
const LINKS = [
  {
    href: "/courses",
    label: "Cursos",
    icon: "📚",
    // Ativo na listagem, no detalhe do curso e nas aulas — mas não na criação.
    match: (p: string) =>
      p !== "/courses/new" && (p === "/courses" || p.startsWith("/courses/") || p.startsWith("/lessons")),
  },
  {
    href: "/courses/new",
    label: "Novo curso",
    icon: "➕",
    teacherOnly: true,
    match: (p: string) => p === "/courses/new",
  },
  {
    href: "/settings",
    label: "Configurações",
    icon: "⚙️",
    match: (p: string) => p === "/settings" || p.startsWith("/settings/"),
  },
] as const;

export function Sidebar() {
  const { status, user } = useAuth();
  const pathname = usePathname();

  // Nada de menu fora da área logada (login, carregamento).
  if (status !== "authenticated" || !user) return null;

  const links = LINKS.filter((link) => !("teacherOnly" in link && link.teacherOnly) || user.role === "Teacher");

  return (
    <aside className="sidebar" aria-label="Navegação principal">
      <nav className="sidebar-nav">
        {links.map((link) => {
          const active = link.match(pathname);
          return (
            <Link
              key={link.href}
              href={link.href}
              aria-current={active ? "page" : undefined}
              className={active ? "sidebar-link sidebar-link-active" : "sidebar-link"}
            >
              <span className="sidebar-icon" aria-hidden>
                {link.icon}
              </span>
              {link.label}
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}
