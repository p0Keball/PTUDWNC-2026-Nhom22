import { Container } from "@/components/Container";
import { DashboardAuthGate } from "@/components/DashboardAuthGate";
import { DashboardNav } from "@/components/DashboardNav";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <Container className="flex-1 gap-6 lg:flex-row lg:items-start">
      <aside className="w-full shrink-0 lg:sticky lg:top-20 lg:w-56">
        <DashboardNav />
      </aside>
      <div className="min-w-0 flex-1">
        <DashboardAuthGate>{children}</DashboardAuthGate>
      </div>
    </Container>
  );
}
