import { Suspense } from "react";
import type { Metadata } from "next";
import { AuthProvider } from "@/components/AuthProvider";
import { Header } from "@/components/Header";
import { Sidebar } from "@/components/Sidebar";
import "./globals.css";

export const metadata: Metadata = {
  title: "Educa+ — aulas em vídeo",
  description: "Plataforma de educação com streaming de vídeo (ODS 4 — Educação de Qualidade).",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="pt-BR">
      <body>
        <AuthProvider>
          <Header />
          <div className="container shell">
            {/* A sidebar lê o caminho atual (usePathname), que só existe em runtime;
                o Suspense deixa o App Shell ser pré-renderizado sem bloquear. */}
            <Suspense fallback={null}>
              <Sidebar />
            </Suspense>
            <main className="main">{children}</main>
          </div>
        </AuthProvider>
      </body>
    </html>
  );
}
