import Link from "next/link";
import { SearchX } from "lucide-react";

import { Container } from "@/components/Container";
import { EmptyState } from "@/components/EmptyState";
import { Button } from "@/components/ui/button";

export default function NotFound() {
  return (
    <Container className="items-center justify-center py-16">
      <EmptyState
        icon={<SearchX className="size-6" />}
        title="404 — Không tìm thấy trang"
        description="Trang bạn truy cập không tồn tại hoặc đã bị di chuyển. Hãy thử tìm công thức khác."
        action={
          <div className="flex flex-wrap justify-center gap-2">
            <Button asChild>
              <Link href="/">Về trang chủ</Link>
            </Button>
            <Button asChild variant="outline">
              <Link href="/recipes">Xem công thức</Link>
            </Button>
          </div>
        }
      />
    </Container>
  );
}
