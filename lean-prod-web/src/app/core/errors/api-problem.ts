export interface ApiProblem { title?: string; detail?: string; status?: number; traceId?: string; errors?: Record<string, string[]>; }
