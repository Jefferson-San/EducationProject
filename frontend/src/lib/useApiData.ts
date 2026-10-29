"use client";

import { useCallback, useEffect, useState } from "react";
import { api, ApiError } from "./api";

interface ApiData<T> {
  data?: T;
  error?: ApiError;
  loading: boolean;
  /** Busca de novo (ex.: depois de criar um item ou no polling do status do vídeo). */
  reload: () => void;
}

/** GET simples na API. Com path nulo, não busca nada (útil quando depende de outro dado). */
export function useApiData<T>(path: string | null): ApiData<T> {
  const [state, setState] = useState<{ data?: T; error?: ApiError; loading: boolean }>({ loading: true });
  const [version, setVersion] = useState(0);

  useEffect(() => {
    if (!path) return;
    let cancelled = false;

    api.get<T>(path).then(
      (data) => !cancelled && setState({ data, loading: false }),
      (error: ApiError) => !cancelled && setState((previous) => ({ ...previous, error, loading: false })),
    );

    return () => {
      cancelled = true;
    };
  }, [path, version]);

  const reload = useCallback(() => setVersion((v) => v + 1), []);
  return { ...state, reload };
}
