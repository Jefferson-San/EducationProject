"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";

/** Página inicial: manda para os cursos (logado) ou para o login. */
export default function Home() {
  const { status } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (status === "authenticated") router.replace("/courses");
    if (status === "anonymous") router.replace("/login");
  }, [status, router]);

  return <p className="muted">Carregando…</p>;
}
