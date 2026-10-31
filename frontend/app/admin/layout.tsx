import type { Metadata } from "next";

import { AdminAuthGate } from "@/components/AdminAuthGate";
import { AdminNav } from "@/components/AdminNav";
import { Container } from "@/components/Container";

export const metadata: Metadata = {
  title: "Quản trị",
  robots: { index: false, follow: false },
};

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <Container className="flex-1 gap-6 lg:flex-row lg:items-start">
      <aside className="w-full shrink-0 lg:sticky lg:top-20 lg:w-56">
        <AdminNav />
      </aside>
      <div className="min-w-0 flex-1">
        <AdminAuthGate>{children}</AdminAuthGate>
      </div>
    </Container>
  );
}
