"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";
import { RequireAuth } from "@/components/RequireAuth";
import { api, ApiError } from "@/lib/api";
import { useApiData } from "@/lib/useApiData";
import type { CourseDetail, CourseSummary } from "@/lib/types";

export default function CoursesPage() {
  return (
    <RequireAuth>
      <Courses />
    </RequireAuth>
  );
}

function Courses() {
  const { user } = useAuth();
  const { data: courses, error, loading } = useApiData<CourseSummary[]>("/v1/courses");

  return (
    <>
      <div className="page-header">
        <h1>Cursos</h1>
      </div>

      {user?.role === "Teacher" && <CreateCourseForm />}

      {loading && <p className="muted">Carregando cursos…</p>}
      {error && <p className="alert alert-error">{error.message}</p>}
      {courses?.length === 0 && (
        <p className="empty">
          Nenhum curso ainda.{user?.role === "Teacher" ? " Crie o primeiro acima." : " Volte mais tarde."}
        </p>
      )}

      <ul className="grid">
        {courses?.map((course) => (
          <li key={course.id}>
            <Link href={`/courses/${course.id}`} className="card card-link">
              <h2 className="card-title">{course.title}</h2>
              {course.description && <p className="card-text">{course.description}</p>}
              <p className="muted card-meta">
                {course.teacherName} · {course.lessonCount} {course.lessonCount === 1 ? "aula" : "aulas"}
                {course.teacherId === user?.id && <span className="tag">Seu curso</span>}
              </p>
            </Link>
          </li>
        ))}
      </ul>
    </>
  );
}

function CreateCourseForm() {
  const router = useRouter();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setSaving(true);
    try {
      const course = await api.post<CourseDetail>("/v1/courses", { title, description: description || undefined });
      // A página fica guardada pelo Next (cacheComponents): limpa o formulário para a próxima visita.
      setTitle("");
      setDescription("");
      router.push(`/courses/${course.id}`);
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Erro inesperado.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <details className="card create">
      <summary className="create-summary">+ Novo curso</summary>
      <form onSubmit={handleSubmit} className="form">
        <label className="field">
          Título
          <input value={title} onChange={(e) => setTitle(e.target.value)} required minLength={3} maxLength={200} />
        </label>
        <label className="field">
          Descrição <span className="muted">(opcional)</span>
          <textarea value={description} onChange={(e) => setDescription(e.target.value)} maxLength={4000} rows={3} />
        </label>
        {error && <p className="alert alert-error">{error}</p>}
        <button type="submit" className="button" disabled={saving}>
          {saving ? "Criando…" : "Criar curso"}
        </button>
      </form>
    </details>
  );
}
