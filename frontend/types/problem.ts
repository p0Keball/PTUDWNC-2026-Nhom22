export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string | null;
  code?: string | null;
  traceId?: string;
  correlationId?: string | null;
  errors?: Record<string, string[]>;
}

export function isProblemDetails(value: unknown): value is ProblemDetails {
  return (
    typeof value === "object" &&
    value !== null &&
    typeof (value as ProblemDetails).status === "number" &&
    typeof (value as ProblemDetails).title === "string"
  );
}
