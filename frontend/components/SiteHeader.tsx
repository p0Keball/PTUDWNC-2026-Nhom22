"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const LINKS = [
  { href: "/", label: "Trang chủ" },
  { href: "/recipes/create", label: "Tạo công thức" },
  { href: "/dashboard/recipes", label: "Của tôi" },
];

export default function SiteHeader() {
  const pathname = usePathname();
  return (
    <header className="sticky top-0 z-10 border-b border-zinc-200 bg-white/90 backdrop-blur dark:border-zinc-800 dark:bg-zinc-950/90">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 p-4 sm:px-6">
        <Link href="/" className="text-lg font-bold">
          Culinary Blog
        </Link>
        <nav className="flex items-center gap-1 text-sm sm:gap-2">
          {LINKS.map((l) => (
            <Link
              key={l.href}
              href={l.href}
              className={`rounded-full px-3 py-1.5 transition hover:bg-zinc-100 dark:hover:bg-zinc-800 ${
                pathname === l.href ? "bg-zinc-100 font-medium dark:bg-zinc-800" : ""
              }`}
            >
              {l.label}
            </Link>
          ))}
        </nav>
      </div>
    </header>
  );
}
