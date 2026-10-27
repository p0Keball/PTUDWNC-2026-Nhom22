"use client";

import Link from "next/link";
import { ChefHat, LogIn, LogOut, UserRound } from "lucide-react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { useAuthStore } from "@/store/auth-store";

export function SiteHeader() {
  const router = useRouter();
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  async function handleLogout() {
    await logout();
    toast.success("Đã đăng xuất.");
    router.push("/");
  }

  return (
    <header className="sticky top-0 z-30 border-b bg-background/95 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-7xl items-center gap-5 px-4 sm:px-6">
        <Link href="/" className="flex items-center gap-2 font-bold text-primary">
          <ChefHat className="size-6" />
          <span>FoodBlog</span>
        </Link>
        <nav className="hidden items-center gap-4 text-sm md:flex">
          <Link href="/recipes" className="hover:text-primary">Công thức</Link>
          <Link href="/categories" className="hover:text-primary">Danh mục</Link>
          <Link href="/search" className="hover:text-primary">Tìm kiếm</Link>
        </nav>
        <div className="ml-auto flex items-center gap-2">
          {user ? (
            <>
              <Link href="/profile" className="hidden items-center gap-2 text-sm sm:flex">
                <UserRound className="size-4" /> {user.fullName}
              </Link>
              <button onClick={handleLogout} className="inline-flex items-center gap-2 rounded-md border px-3 py-2 text-sm hover:bg-muted">
                <LogOut className="size-4" /> <span className="hidden sm:inline">Đăng xuất</span>
              </button>
            </>
          ) : (
            <Link href="/auth/login" className="inline-flex items-center gap-2 rounded-md border px-3 py-2 text-sm hover:bg-muted">
              <LogIn className="size-4" /> Đăng nhập
            </Link>
          )}
        </div>
      </div>
    </header>
  );
}
