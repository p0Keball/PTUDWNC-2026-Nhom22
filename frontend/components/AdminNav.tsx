"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { ArrowLeft, ScrollText, Users } from "lucide-react";

import { cn } from "@/lib/utils";

const LINKS = [
  { href: "/admin/users", label: "Người dùng", icon: Users },
  { href: "/admin/logs", label: "Nhật ký hệ thống", icon: ScrollText },
];

export function AdminNav() {
  const pathname = usePathname();

  return (
    <nav
      aria-label="Điều hướng quản trị"
      className="flex w-full flex-col gap-1"
    >
      <p className="px-3 pb-1 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
        Quản trị
      </p>
      {LINKS.map((link) => {
        const active = pathname.startsWith(link.href);
        return (
          <Link
            key={link.href}
            href={link.href}
            aria-current={active ? "page" : undefined}
            className={cn(
              "inline-flex items-center gap-2 rounded-lg px-3 py-2 text-sm transition-colors hover:bg-accent hover:text-accent-foreground",
              active && "bg-secondary font-medium text-secondary-foreground"
            )}
          >
            <link.icon className="size-4" />
            {link.label}
          </Link>
        );
      })}
      <Link
        href="/dashboard"
        className="mt-2 inline-flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
      >
        <ArrowLeft className="size-4" />
        Về dashboard
      </Link>
    </nav>
  );
}
