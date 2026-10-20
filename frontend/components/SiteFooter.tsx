import Link from "next/link";
import { ChefHat } from "lucide-react";

const FOOTER_LINKS = [
  {
    title: "Khám phá",
    links: [
      { href: "/", label: "Trang chủ" },
      { href: "/recipes", label: "Tất cả công thức" },
    ],
  },
  {
    title: "Tác giả",
    links: [
      { href: "/dashboard", label: "Dashboard" },
      { href: "/dashboard/recipes", label: "Công thức của tôi" },
      { href: "/dashboard/recipes/new", label: "Viết công thức mới" },
    ],
  },
];

export default function SiteFooter() {
  return (
    <footer className="mt-auto border-t bg-muted/30">
      <div className="mx-auto grid w-full max-w-6xl gap-8 px-4 py-10 sm:px-6 md:grid-cols-[1.5fr_1fr_1fr]">
        <div className="flex flex-col gap-3">
          <Link href="/" className="flex items-center gap-2 font-bold">
            <span className="flex size-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
              <ChefHat className="size-4" />
            </span>
            Culinary Blog
          </Link>
          <p className="max-w-sm text-sm text-muted-foreground">
            Blog Ẩm thực và Nấu ăn — nơi chia sẻ công thức từ nhiều nền ẩm thực,
            giúp mọi người nấu ăn dễ dàng hơn mỗi ngày.
          </p>
        </div>

        {FOOTER_LINKS.map((group) => (
          <div key={group.title} className="flex flex-col gap-2 text-sm">
            <p className="font-medium">{group.title}</p>
            {group.links.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className="w-fit text-muted-foreground transition-colors hover:text-foreground"
              >
                {link.label}
              </Link>
            ))}
          </div>
        ))}
      </div>

      <div className="border-t">
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-1 px-4 py-4 text-xs text-muted-foreground sm:flex-row sm:items-center sm:justify-between sm:px-6">
          <p>© {new Date().getFullYear()} Culinary Blog — Nhóm 22.</p>
          <p>Xây dựng với Next.js, Tailwind CSS và shadcn/ui.</p>
        </div>
      </div>
    </footer>
  );
}
