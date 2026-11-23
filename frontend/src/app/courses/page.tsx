"use client";

import { useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/AuthProvider";
import { RequireAuth } from "@/components/RequireAuth";
import { COURSE_CATEGORIES, categoryLabel } from "@/lib/categories";
import { useApiData } from "@/lib/useApiData";
import type { CourseCategory, CourseSummary } from "@/lib/types";

export default function CoursesPage() {
  return (
    <RequireAuth>
      <Courses />
    </RequireAuth>
  );
}

/** "Todas" é o filtro sem categoria: a lista completa. */
type Filter = CourseCategory | "Todas";

function Courses() {
  const { user } = useAuth();
  const [filter, setFilter] = useState<Filter>("Todas");

  const path = filter === "Todas" ? "/v1/courses" : `/v1/courses?category=${filter}`;
  const { data: courses, error, loading } = useApiData<CourseSummary[]>(path);

  return (
    <>
      <div className="page-header">
        <h1>Cursos</h1>
        {user?.role === "Teacher" && (
          <Link href="/courses/new" className="button">
            + Novo curso
          </Link>
        )}
      </div>

      <div className="chips" role="group" aria-label="Filtrar por categoria">
        {(["Todas", ...COURSE_CATEGORIES.map((c) => c.value)] as Filter[]).map((value) => (
          <button
            key={value}
            type="button"
            aria-pressed={filter === value}
            className={filter === value ? "chip chip-active" : "chip"}
            onClick={() => setFilter(value)}
          >
            {value === "Todas" ? "Todas" : categoryLabel(value)}
          </button>
        ))}
      </div>

      {loading && <p className="muted">Carregando cursos…</p>}
      {error && <p className="alert alert-error">{error.message}</p>}
      {courses?.length === 0 && (
        <p className="empty">
          {filter === "Todas"
            ? user?.role === "Teacher"
              ? "Nenhum curso ainda. Clique em “Novo curso” para criar o primeiro."
              : "Nenhum curso ainda. Volte mais tarde."
            : `Nenhum curso de ${categoryLabel(filter)} ainda.`}
        </p>
      )}

      <ul className="grid">
        {courses?.map((course) => (
          <li key={course.id}>
            <Link href={`/courses/${course.id}`} className="card card-link">
              <span className="tag tag-category">{categoryLabel(course.category)}</span>
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
