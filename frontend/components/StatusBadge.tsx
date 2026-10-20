import { Badge } from "@/components/ui/badge";
import { statusLabel } from "@/lib/utils";

export function StatusBadge({ status }: { status: string }) {
  const normalized = status.toLowerCase();
  const variant =
    normalized === "published"
      ? "success"
      : normalized === "archived"
        ? "warning"
        : "secondary";

  return <Badge variant={variant}>{statusLabel(status)}</Badge>;
}
