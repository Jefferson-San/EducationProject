import type { CourseCategory } from "./types";

/** Mesma ordem e valores do enum CourseCategory do backend. */
export const COURSE_CATEGORIES: { value: CourseCategory; label: string }[] = [
  { value: "Matematica", label: "Matemática" },
  { value: "Ciencias", label: "Ciências" },
  { value: "Portugues", label: "Português" },
  { value: "Historia", label: "História" },
  { value: "Geografia", label: "Geografia" },
  { value: "Artes", label: "Artes" },
  { value: "Outros", label: "Outros" },
];

export const categoryLabel = (value: CourseCategory) =>
  COURSE_CATEGORIES.find((c) => c.value === value)?.label ?? value;
