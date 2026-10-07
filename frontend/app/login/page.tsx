import Link from "next/link";
import GoogleLoginButton from "@/components/GoogleLoginButton";

export const metadata = {
  title: "Đăng nhập — Culinary Blog",
};

export default function LoginPage() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-zinc-950 px-5 py-12">
      <section className="w-full max-w-md rounded-3xl bg-white p-8 shadow-2xl sm:p-10">
        <Link href="/" className="text-sm font-semibold text-zinc-500 hover:text-zinc-900">
          Culinary Blog
        </Link>
        <h1 className="mt-10 text-3xl font-bold tracking-tight text-zinc-950">Chào mừng trở lại</h1>
        <p className="mt-3 text-sm leading-6 text-zinc-500">
          Đăng nhập để lưu công thức và chia sẻ món ăn của bạn.
        </p>
        <div className="mt-8">
          <GoogleLoginButton />
        </div>
        <p className="mt-8 text-center text-xs leading-5 text-zinc-400">
          Tài khoản Google đã có trong hệ thống sẽ được liên kết tự động theo email đã xác thực.
        </p>
      </section>
    </main>
  );
}