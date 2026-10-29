import type { VideoStatus } from "@/lib/types";

const LABELS: Record<VideoStatus, string> = {
  Uploaded: "Na fila",
  Processing: "Processando",
  Processed: "Disponível",
  Failed: "Falhou",
};

export function StatusBadge({ status }: { status?: VideoStatus }) {
  if (!status) return <span className="badge badge-empty">Sem vídeo</span>;
  return <span className={`badge badge-${status.toLowerCase()}`}>{LABELS[status]}</span>;
}
