import type { Metadata } from "next";
import { AuthProvider } from "@/components/AuthProvider";
import { Header } from "@/components/Header";
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
          <main className="container main">{children}</main>
        </AuthProvider>
      </body>
    </html>
  );
}
