"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { ChefHat, LogOut, PenLine, UserRound } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/store/auth-store";

const LINKS = [
  { href: "/", label: "Trang chủ" },
  { href: "/recipes", label: "Công thức" },
  { href: "/dashboard", label: "Dashboard" },
];

export default function SiteHeader() {
  const pathname = usePathname();
  const router = useRouter();
  const user = useAuthStore((state) => state.user);
  const status = useAuthStore((state) => state.status);
  const hasHydrated = useAuthStore((state) => state.hasHydrated);
  const logout = useAuthStore((state) => state.logout);

  const isActive = (href: string) =>
    href === "/" ? pathname === "/" : pathname.startsWith(href);

  async function handleLogout() {
    await logout();
    toast.success("Đã đăng xuất.");
    router.push("/");
    router.refresh();
  }

  return (
    <header className="sticky top-0 z-40 border-b bg-background/90 backdrop-blur supports-[backdrop-filter]:bg-background/70">
      <div className="mx-auto flex w-full max-w-6xl flex-wrap items-center justify-between gap-3 px-4 py-3 sm:px-6">
        <Link
          href="/"
          className="flex items-center gap-2 text-lg font-bold tracking-tight"
        >
          <span className="flex size-9 items-center justify-center rounded-xl bg-primary text-primary-foreground">
            <ChefHat className="size-5" />
          </span>
          <span>
            Culinary<span className="text-muted-foreground">Blog</span>
          </span>
        </Link>

        <nav className="order-3 flex w-full items-center gap-1 overflow-x-auto text-sm sm:order-none sm:w-auto">
          {LINKS.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={cn(
                "rounded-full px-3 py-1.5 whitespace-nowrap transition-colors hover:bg-accent hover:text-accent-foreground",
                isActive(link.href) &&
                  "bg-secondary font-medium text-secondary-foreground"
              )}
            >
              {link.label}
            </Link>
          ))}
        </nav>

        <div className="flex items-center gap-2">
          {hasHydrated && status === "authenticated" && user ? (
            <>
              <span className="hidden items-center gap-1.5 text-sm text-muted-foreground sm:inline-flex">
                <UserRound className="size-4" />
                {user.fullName}
              </span>
              <Button variant="ghost" size="sm" onClick={handleLogout}>
                <LogOut />
                <span className="hidden sm:inline">Đăng xuất</span>
              </Button>
            </>
          ) : (
            <Button asChild variant="ghost" size="sm">
              <Link href="/auth/login">Đăng nhập</Link>
            </Button>
          )}

          <Button asChild size="sm">
            <Link href="/dashboard/recipes/new">
              <PenLine />
              <span className="hidden sm:inline">Viết công thức</span>
              <span className="sm:hidden">Viết</span>
            </Link>
          </Button>
        </div>
      </div>
    </header>
  );
}
