"use client";

import { useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ApiError, login } from "@/lib/api";
import { appConfig } from "@/lib/config";

function base64Url(bytes: Uint8Array) {
  let binary = "";
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte);
  });
  return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

async function createPkcePair() {
  const verifier = base64Url(crypto.getRandomValues(new Uint8Array(32)));
  const digest = await crypto.subtle.digest(
    "SHA-256",
    new TextEncoder().encode(verifier)
  );
  return { verifier, challenge: base64Url(new Uint8Array(digest)) };
}

function saveAuth(accessToken: string, refreshToken: string) {
  localStorage.setItem("accessToken", accessToken);
  localStorage.setItem("refreshToken", refreshToken);
}

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      const result = await login(email.trim(), password);
      if (result.accessToken) saveAuth(result.accessToken, result.refreshToken);
      else localStorage.setItem("refreshToken", result.refreshToken);
      router.push("/profile");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Đăng nhập thất bại.");
    } finally {
      setSubmitting(false);
    }
  };

  const googleLogin = async () => {
    const clientId = appConfig.googleClientId;
    if (!clientId) {
      setError("Chưa cấu hình Google Login cho frontend.");
      return;
    }
    const redirectUri = `${window.location.origin}/auth/google/callback`;
    const { verifier, challenge } = await createPkcePair();
    const state = base64Url(crypto.getRandomValues(new Uint8Array(24)));
    sessionStorage.setItem("google_pkce_verifier", verifier);
    sessionStorage.setItem("google_oauth_state", state);
    const params = new URLSearchParams({
      client_id: clientId,
      redirect_uri: redirectUri,
      response_type: "code",
      scope: "openid email profile",
      code_challenge: challenge,
      code_challenge_method: "S256",
      state,
      access_type: "offline",
      include_granted_scopes: "true",
    });
    window.location.assign(`https://accounts.google.com/o/oauth2/v2/auth?${params}`);
  };

  return (
    <main className="flex flex-1 items-center justify-center bg-orange-50 px-4 py-12 dark:bg-zinc-950">
      <section className="w-full max-w-md rounded-3xl bg-white p-8 shadow-xl dark:bg-zinc-900">
        <div className="mb-8 text-center">
          <p className="mb-2 text-sm font-semibold uppercase tracking-[0.2em] text-orange-500">FoodBlog</p>
          <h1 className="text-3xl font-bold">Chào mừng trở lại</h1>
          <p className="mt-2 text-sm text-zinc-500">Đăng nhập để quản lý hồ sơ và công thức của bạn.</p>
        </div>
        <button
          type="button"
          onClick={googleLogin}
          className="flex w-full items-center justify-center gap-3 rounded-xl border border-zinc-200 px-4 py-3 font-semibold hover:bg-zinc-50 dark:border-zinc-700 dark:hover:bg-zinc-800"
        >
          <span className="text-lg font-bold text-blue-600">G</span>
          Đăng nhập bằng Google
        </button>
        <div className="my-6 flex items-center gap-3 text-xs text-zinc-400">
          <span className="h-px flex-1 bg-zinc-200 dark:bg-zinc-700" />
          HOẶC
          <span className="h-px flex-1 bg-zinc-200 dark:bg-zinc-700" />
        </div>
        <form onSubmit={handleSubmit} className="space-y-4">
          <label className="block text-sm font-medium">
            Email
            <input required type="email" value={email} onChange={(e) => setEmail(e.target.value)} className="mt-1 w-full rounded-xl border border-zinc-200 px-4 py-3 outline-none focus:border-orange-500 dark:border-zinc-700 dark:bg-zinc-800" />
          </label>
          <label className="block text-sm font-medium">
            Mật khẩu
            <input required type="password" value={password} onChange={(e) => setPassword(e.target.value)} className="mt-1 w-full rounded-xl border border-zinc-200 px-4 py-3 outline-none focus:border-orange-500 dark:border-zinc-700 dark:bg-zinc-800" />
          </label>
          {error && <p role="alert" className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{error}</p>}
          <button disabled={submitting} className="w-full rounded-xl bg-orange-500 px-4 py-3 font-semibold text-white hover:bg-orange-600 disabled:cursor-not-allowed disabled:opacity-60">
            {submitting ? "Đang đăng nhập..." : "Đăng nhập"}
          </button>
        </form>
        <p className="mt-6 text-center text-sm text-zinc-500">
          Chưa có tài khoản? <Link href="/register" className="font-semibold text-orange-600 hover:underline">Đăng ký</Link>
        </p>
      </section>
    </main>
  );
}
