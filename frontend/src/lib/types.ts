// Contratos da Education API (/v1). Campos nulos não vêm no JSON, por isso são opcionais.

export type UserRole = "Teacher" | "Student";

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  user: User;
}

export type CourseCategory =
  | "Matematica"
  | "Ciencias"
  | "Portugues"
  | "Historia"
  | "Geografia"
  | "Artes"
  | "Outros";

export type VideoStatus = "Uploaded" | "Processing" | "Processed" | "Failed";

export interface CourseSummary {
  id: string;
  title: string;
  description?: string;
  category: CourseCategory;
  teacherId: string;
  teacherName: string;
  lessonCount: number;
  createdAt: string;
}

export interface LessonSummary {
  id: string;
  title: string;
  description?: string;
  order: number;
  videoStatus?: VideoStatus;
}

export interface CourseDetail {
  id: string;
  title: string;
  description?: string;
  category: CourseCategory;
  teacherId: string;
  teacherName: string;
  createdAt: string;
  lessons: LessonSummary[];
}

export interface Video {
  id: string;
  status: VideoStatus;
  /** URL absoluta do master.m3u8 no serviço de mídia (Nginx). Só existe quando Processed. */
  streamingUrl?: string;
  durationSeconds?: number;
  failureReason?: string;
  createdAt: string;
}

export interface Lesson {
  id: string;
  courseId: string;
  title: string;
  description?: string;
  order: number;
  video?: Video;
}

/** Formato de erro de toda a API: { statusCode, message, code }. */
export interface ApiErrorResponse {
  statusCode: number;
  message: string;
  code: string;
}
