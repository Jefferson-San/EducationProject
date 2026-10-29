"use client";

import { useEffect, useRef, useState } from "react";
import Hls from "hls.js";

interface Quality {
  index: number;
  label: string;
}

/**
 * Player HLS. Recebe o master.m3u8 (servido pelo Nginx) e o hls.js baixa os segmentos sob demanda,
 * trocando de qualidade conforme a conexão. Safari toca HLS nativamente, sem o hls.js.
 */
export function VideoPlayer({ src }: { src: string }) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const hlsRef = useRef<Hls | null>(null);
  const [qualities, setQualities] = useState<Quality[]>([]);
  const [selected, setSelected] = useState(-1); // -1 = automático
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    if (Hls.isSupported()) {
      const hls = new Hls();
      hlsRef.current = hls;

      hls.on(Hls.Events.MANIFEST_PARSED, (_, data) => {
        setQualities(data.levels.map((level, index) => ({ index, label: `${level.height}p` })));
      });
      hls.on(Hls.Events.ERROR, (_, data) => {
        if (data.fatal) setError("Não foi possível carregar o vídeo. Verifique se o serviço de mídia está no ar.");
      });

      hls.loadSource(src);
      hls.attachMedia(video);
      return () => {
        hls.destroy();
        hlsRef.current = null;
      };
    }

    if (video.canPlayType("application/vnd.apple.mpegurl")) {
      video.src = src; // Safari / iOS
    }
  }, [src]);

  function changeQuality(level: number) {
    setSelected(level);
    if (hlsRef.current) hlsRef.current.currentLevel = level;
  }

  return (
    <div className="player">
      <video ref={videoRef} controls playsInline className="player-video" />
      {error && <p className="alert alert-error">{error}</p>}
      {qualities.length > 1 && (
        <label className="player-quality">
          Qualidade:{" "}
          <select value={selected} onChange={(e) => changeQuality(Number(e.target.value))}>
            <option value={-1}>Automática</option>
            {qualities.map((q) => (
              <option key={q.index} value={q.index}>
                {q.label}
              </option>
            ))}
          </select>
        </label>
      )}
    </div>
  );
}
