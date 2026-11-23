"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";
import { RequireAuth } from "@/components/RequireAuth";
import { api, ApiError } from "@/lib/api";
import { COURSE_CATEGORIES } from "@/lib/categories";
import type { CourseCategory, CourseDetail } from "@/lib/types";

export default function NewCoursePage() {
  return (
    <RequireAuth>
      <NewCourse />
    </RequireAuth>
  );
}

function NewCourse() {
  const { user } = useAuth();

  // Só professores criam cursos — alunos veem um aviso em vez do formulário.
  if (user?.role !== "Teacher") {
    return (
      <>
        <div className="breadcrumb">
          <Link href="/courses">← Cursos</Link>
        </div>
        <div className="page-header">
          <h1>Novo curso</h1>
        </div>
        <p className="empty">Apenas professores podem criar cursos.</p>
      </>
    );
  }

  return <CreateCourseForm />;
}

function CreateCourseForm() {
  const router = useRouter();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [category, setCategory] = useState<CourseCategory | "">("");
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!category) return setError("Escolha a categoria do curso.");
    setError(null);
    setSaving(true);
    try {
      const course = await api.post<CourseDetail>("/v1/courses", {
        title,
        description: description || undefined,
        category,
      });
      // Deu certo: navega para o curso criado (esta página é desmontada).
      router.push(`/courses/${course.id}`);
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Erro inesperado.");
      setSaving(false);
    }
  }

  return (
    <>
      <div className="breadcrumb">
        <Link href="/courses">← Cursos</Link>
      </div>
      <div className="page-header">
        <h1>Novo curso</h1>
      </div>
      <p className="lead muted">Preencha os dados abaixo para publicar um novo curso.</p>

      <form onSubmit={handleSubmit} className="card form">
        <label className="field">
          Título
          <input value={title} onChange={(e) => setTitle(e.target.value)} required minLength={3} maxLength={200} />
        </label>
        <label className="field">
          Categoria
          <select value={category} onChange={(e) => setCategory(e.target.value as CourseCategory)} required>
            <option value="" disabled>
              Escolha uma categoria
            </option>
            {COURSE_CATEGORIES.map((c) => (
              <option key={c.value} value={c.value}>
                {c.label}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          Descrição <span className="muted">(opcional)</span>
          <textarea value={description} onChange={(e) => setDescription(e.target.value)} maxLength={4000} rows={4} />
        </label>
        {error && <p className="alert alert-error">{error}</p>}
        <div className="form-actions">
          <button type="submit" className="button" disabled={saving}>
            {saving ? "Criando…" : "Criar curso"}
          </button>
          <Link href="/courses" className="button button-ghost">
            Cancelar
          </Link>
        </div>
      </form>
    </>
  );
}
