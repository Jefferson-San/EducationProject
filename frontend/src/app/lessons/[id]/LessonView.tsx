"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";
import { RequireAuth } from "@/components/RequireAuth";
import { StatusBadge } from "@/components/StatusBadge";
import { VideoPlayer } from "@/components/VideoPlayer";
import { ApiError, uploadFile } from "@/lib/api";
import { MAX_VIDEO_BYTES, VIDEO_POLLING_MS } from "@/lib/config";
import { useApiData } from "@/lib/useApiData";
import type { CourseDetail, Lesson, Video } from "@/lib/types";

export function LessonView() {
  return (
    <RequireAuth>
      <LessonContent />
    </RequireAuth>
  );
}

function LessonContent() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const lesson = useApiData<Lesson>(`/v1/lessons/${id}`);
  const course = useApiData<CourseDetail>(lesson.data ? `/v1/courses/${lesson.data.courseId}` : null);

  const status = lesson.data?.video?.status;
  const pending = status === "Uploaded" || status === "Processing";
  const { reload } = lesson;

  // Polling enquanto o Worker processa o vídeo (Uploaded → Processing → Processed/Failed).
  useEffect(() => {
    if (!pending) return;
    const timer = setInterval(reload, VIDEO_POLLING_MS);
    return () => clearInterval(timer);
  }, [pending, reload]);

  if (lesson.loading) return <p className="muted">Carregando aula…</p>;
  if (lesson.error || !lesson.data) return <p className="alert alert-error">{lesson.error?.message ?? "Aula não encontrada."}</p>;

  const data = lesson.data;
  const isOwner = !!course.data && course.data.teacherId === user?.id;

  return (
    <>
      <nav className="breadcrumb">
        <Link href="/courses">Cursos</Link> /{" "}
        <Link href={`/courses/${data.courseId}`}>{course.data?.title ?? "Curso"}</Link> / <span>{data.title}</span>
      </nav>

      <div className="page-header">
        <div>
          <p className="muted">Aula {data.order}</p>
          <h1>{data.title}</h1>
        </div>
        <StatusBadge status={status} />
      </div>
      {data.description && <p className="lead">{data.description}</p>}

      <VideoArea video={data.video} isOwner={isOwner} />

      {isOwner && <UploadVideoForm lessonId={data.id} hasVideo={!!data.video} disabled={status === "Processing"} onUploaded={reload} />}
    </>
  );
}

function VideoArea({ video, isOwner }: { video?: Video; isOwner: boolean }) {
  if (!video) {
    return (
      <div className="video-placeholder">
        <p>{isOwner ? "Esta aula ainda não tem vídeo. Envie um abaixo." : "O vídeo desta aula ainda não foi publicado."}</p>
      </div>
    );
  }

  switch (video.status) {
    case "Processed":
      return (
        <>
          <VideoPlayer src={video.streamingUrl!} />
          {video.durationSeconds && <p className="muted">Duração: {formatDuration(video.durationSeconds)}</p>}
        </>
      );
    case "Failed":
      return (
        <div className="video-placeholder video-failed">
          <p>
            <strong>Não foi possível processar o vídeo.</strong>
          </p>
          {video.failureReason && <p className="muted">{video.failureReason}</p>}
          {isOwner && <p>Envie o vídeo novamente abaixo.</p>}
        </div>
      );
    default:
      return (
        <div className="video-placeholder">
          <span className="spinner" aria-hidden />
          <p>{video.status === "Uploaded" ? "Vídeo na fila de processamento…" : "Convertendo o vídeo para 360p, 480p e 720p…"}</p>
          <p className="muted">Esta página atualiza sozinha.</p>
        </div>
      );
  }
}

function UploadVideoForm({
  lessonId,
  hasVideo,
  disabled,
  onUploaded,
}: {
  lessonId: string;
  hasVideo: boolean;
  disabled: boolean;
  onUploaded: () => void;
}) {
  const [file, setFile] = useState<File | null>(null);
  const [progress, setProgress] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!file) return;
    setError(null);

    if (!file.name.toLowerCase().endsWith(".mp4")) return setError("Apenas arquivos .mp4 são aceitos.");
    if (file.size > MAX_VIDEO_BYTES) return setError("O arquivo deve ter no máximo 500 MB.");

    setProgress(0);
    try {
      await uploadFile<Video>(`/v1/lessons/${lessonId}/video`, file, setProgress);
      setFile(null);
      (event.target as HTMLFormElement).reset();
      onUploaded();
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Erro inesperado no envio.");
    } finally {
      setProgress(null);
    }
  }

  const uploading = progress !== null;

  return (
    <form onSubmit={handleSubmit} className="card form upload">
      <h3>{hasVideo ? "Substituir vídeo" : "Enviar vídeo"}</h3>
      {disabled ? (
        <p className="muted">Aguarde o processamento atual terminar para enviar outro vídeo.</p>
      ) : (
        <>
          <label className="field">
            Arquivo .mp4 (até 500 MB)
            <input type="file" accept="video/mp4,.mp4" onChange={(e) => setFile(e.target.files?.[0] ?? null)} disabled={uploading} required />
          </label>
          {hasVideo && <p className="hint">O vídeo atual será substituído e processado de novo.</p>}

          {uploading && (
            <div className="progress" role="progressbar" aria-valuenow={progress} aria-valuemin={0} aria-valuemax={100}>
              <div className="progress-bar" style={{ width: `${progress}%` }} />
              <span className="progress-label">{progress < 100 ? `Enviando… ${progress}%` : "Finalizando…"}</span>
            </div>
          )}
          {error && <p className="alert alert-error">{error}</p>}

          <button type="submit" className="button" disabled={!file || uploading}>
            {uploading ? "Enviando…" : "Enviar"}
          </button>
        </>
      )}
    </form>
  );
}

function formatDuration(totalSeconds: number) {
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = Math.round(totalSeconds % 60);
  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
}
