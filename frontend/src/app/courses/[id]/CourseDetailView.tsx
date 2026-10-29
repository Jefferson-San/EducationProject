"use client";

import { useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";
import { RequireAuth } from "@/components/RequireAuth";
import { StatusBadge } from "@/components/StatusBadge";
import { api, ApiError } from "@/lib/api";
import { useApiData } from "@/lib/useApiData";
import type { CourseDetail, Lesson } from "@/lib/types";

export function CourseDetailView() {
  return (
    <RequireAuth>
      <Course />
    </RequireAuth>
  );
}

function Course() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const { data: course, error, loading, reload } = useApiData<CourseDetail>(`/v1/courses/${id}`);

  if (loading) return <p className="muted">Carregando curso…</p>;
  if (error || !course) return <p className="alert alert-error">{error?.message ?? "Curso não encontrado."}</p>;

  const isOwner = course.teacherId === user?.id;

  return (
    <>
      <nav className="breadcrumb">
        <Link href="/courses">Cursos</Link> / <span>{course.title}</span>
      </nav>

      <div className="page-header">
        <div>
          <h1>{course.title}</h1>
          <p className="muted">Prof. {course.teacherName}</p>
        </div>
      </div>
      {course.description && <p className="lead">{course.description}</p>}

      <h2 className="section-title">Aulas</h2>
      {course.lessons.length === 0 && (
        <p className="empty">Este curso ainda não tem aulas.{isOwner && " Adicione a primeira abaixo."}</p>
      )}
      <ol className="lessons">
        {course.lessons.map((lesson) => (
          <li key={lesson.id}>
            <Link href={`/lessons/${lesson.id}`} className="lesson">
              <span className="lesson-order">{lesson.order}</span>
              <span className="lesson-body">
                <span className="lesson-title">{lesson.title}</span>
                {lesson.description && <span className="muted lesson-description">{lesson.description}</span>}
              </span>
              <StatusBadge status={lesson.videoStatus} />
            </Link>
          </li>
        ))}
      </ol>

      {isOwner && <CreateLessonForm courseId={course.id} onCreated={reload} />}
    </>
  );
}

function CreateLessonForm({ courseId, onCreated }: { courseId: string; onCreated: () => void }) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setSaving(true);
    try {
      // Sem "order": a aula vai para o final do curso.
      await api.post<Lesson>(`/v1/courses/${courseId}/lessons`, { title, description: description || undefined });
      setTitle("");
      setDescription("");
      onCreated();
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Erro inesperado.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="card form">
      <h3>Nova aula</h3>
      <label className="field">
        Título
        <input value={title} onChange={(e) => setTitle(e.target.value)} required minLength={3} maxLength={200} />
      </label>
      <label className="field">
        Descrição <span className="muted">(opcional)</span>
        <textarea value={description} onChange={(e) => setDescription(e.target.value)} maxLength={4000} rows={2} />
      </label>
      {error && <p className="alert alert-error">{error}</p>}
      <button type="submit" className="button" disabled={saving}>
        {saving ? "Salvando…" : "Adicionar aula"}
      </button>
      <p className="hint">Depois de criar, abra a aula para enviar o vídeo.</p>
    </form>
  );
}
