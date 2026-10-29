import { Suspense } from "react";
import { CourseDetailView } from "./CourseDetailView";

// Com cacheComponents, o id da rota só é conhecido na requisição: useParams precisa de um Suspense acima.
export default function CoursePage() {
  return (
    <Suspense fallback={<p className="muted">Carregando curso…</p>}>
      <CourseDetailView />
    </Suspense>
  );
}
