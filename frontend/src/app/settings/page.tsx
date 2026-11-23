"use client";

import { useAuth } from "@/components/AuthProvider";
import { RequireAuth } from "@/components/RequireAuth";

const ROLE_LABEL = { Teacher: "Professor", Student: "Aluno" } as const;

export default function SettingsPage() {
  return (
    <RequireAuth>
      <Settings />
    </RequireAuth>
  );
}

function Settings() {
  const { user } = useAuth();
  if (!user) return null;

  return (
    <>
      <div className="page-header">
        <h1>Configurações</h1>
      </div>
      <p className="lead muted">Dados da sua conta.</p>

      <div className="card">
        <dl className="info-list">
          <div className="info-row">
            <dt>Nome</dt>
            <dd>{user.name}</dd>
          </div>
          <div className="info-row">
            <dt>E-mail</dt>
            <dd>{user.email}</dd>
          </div>
          <div className="info-row">
            <dt>Perfil</dt>
            <dd>{ROLE_LABEL[user.role]}</dd>
          </div>
        </dl>
        <p className="hint info-note">A edição dos dados da conta estará disponível em breve.</p>
      </div>
    </>
  );
}
