"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  BookOpen,
  LayoutDashboard,
  PenLine,
  ScrollText,
  ShieldCheck,
  Users,
} from "lucide-react";

import { cn } from "@/lib/utils";
import { useAuthStore } from "@/store/auth-store";

const LINKS = [
  { href: "/dashboard", label: "Tổng quan", icon: LayoutDashboard, exact: true },
  { href: "/dashboard/recipes", label: "Công thức của tôi", icon: BookOpen },
  { href: "/dashboard/recipes/new", label: "Viết công thức", icon: PenLine },
];

const ADMIN_LINKS = [
  { href: "/admin/users", label: "Quản lý người dùng", icon: Users },
  { href: "/admin/logs", label: "Nhật ký hệ thống", icon: ScrollText },
];

export function DashboardNav() {
  const pathname = usePathname();
  const user = useAuthStore((state) => state.user);
  const isAdmin =
    user?.role === "Admin" || Boolean(user?.roles?.includes("Admin"));

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center gap-3 rounded-xl border bg-card p-3">
        <div className="flex size-10 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-semibold text-primary-foreground">
          {user?.fullName?.trim().charAt(0).toUpperCase() || "K"}
        </div>
        <div className="min-w-0">
          <p className="truncate text-sm font-medium">
            {user?.fullName ?? "Tác giả"}
          </p>
          <p className="truncate text-xs text-muted-foreground">
            {user?.email ?? "Chưa đăng nhập"}
          </p>
        </div>
      </div>

      <nav className="flex gap-1 overflow-x-auto lg:flex-col">
        {[...LINKS, ...(isAdmin ? ADMIN_LINKS : [])].map((link) => {
          const active = "exact" in link && link.exact
            ? pathname === link.href
            : pathname.startsWith(link.href);
          return (
            <Link
              key={link.href}
              href={link.href}
              className={cn(
                "inline-flex items-center gap-2 whitespace-nowrap rounded-lg px-3 py-2 text-sm transition-colors hover:bg-accent hover:text-accent-foreground",
                active && "bg-secondary font-medium text-secondary-foreground"
              )}
            >
              <link.icon className="size-4" />
              {link.label}
            </Link>
          );
        })}
      </nav>

      {isAdmin && (
        <Link
          href="/admin/users"
          className="inline-flex items-center gap-2 rounded-lg border border-dashed px-3 py-2 text-xs text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <ShieldCheck className="size-4" />
          Khu vực quản trị
        </Link>
      )}
    </div>
  );
}
