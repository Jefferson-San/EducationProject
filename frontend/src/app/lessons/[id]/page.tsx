import { Suspense } from "react";
import { LessonView } from "./LessonView";

// Com cacheComponents, o id da rota só é conhecido na requisição: useParams precisa de um Suspense acima.
export default function LessonPage() {
  return (
    <Suspense fallback={<p className="muted">Carregando aula…</p>}>
      <LessonView />
    </Suspense>
  );
}
