/** URL da Education API. Embutida no bundle em tempo de build (prefixo NEXT_PUBLIC_). */
export const API_URL = (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080").replace(/\/$/, "");

/** Intervalo de polling enquanto o vídeo está em processamento. */
export const VIDEO_POLLING_MS = 3000;

/** Mesmo limite do backend (UploadVideoCommand.MaxFileSizeBytes). */
export const MAX_VIDEO_BYTES = 500 * 1024 * 1024;
