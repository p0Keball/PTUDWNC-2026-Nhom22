import Link from "next/link";

export function SiteFooter() {
  return (
    <footer className="mt-auto border-t">
      <div className="mx-auto flex max-w-7xl flex-col gap-3 px-4 py-8 text-sm text-muted-foreground sm:flex-row sm:items-center sm:justify-between sm:px-6">
        <p>© {new Date().getFullYear()} FoodBlog. Chia sẻ món ngon mỗi ngày.</p>
        <div className="flex gap-4">
          <Link href="/recipes">Công thức</Link>
          <Link href="/categories">Danh mục</Link>
          <Link href="/auth/register">Đăng ký</Link>
        </div>
      </div>
    </footer>
  );
}
