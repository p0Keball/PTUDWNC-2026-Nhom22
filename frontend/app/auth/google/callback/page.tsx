"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { loginWithGoogle } from "@/lib/api";

export default function GoogleCallbackPage() {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const query = new URLSearchParams(window.location.search);
    const code = query.get("code");
    const state = query.get("state");
    const expectedState = sessionStorage.getItem("google_oauth_state");
    const codeVerifier = sessionStorage.getItem("google_pkce_verifier");
    if (!code || !codeVerifier || !state || state !== expectedState) {
      const timer = window.setTimeout(
        () => setError("Google OAuth state hoặc authorization code không hợp lệ."),
        0
      );
      return () => window.clearTimeout(timer);
    }
    const redirectUri = `${window.location.origin}/auth/google/callback`;
    loginWithGoogle(code, codeVerifier, redirectUri)
      .then((result) => {
        sessionStorage.removeItem("google_pkce_verifier");
        sessionStorage.removeItem("google_oauth_state");
        if (result.accessToken) localStorage.setItem("accessToken", result.accessToken);
        localStorage.setItem("refreshToken", result.refreshToken);
        router.replace("/profile");
      })
      .catch(() => setError("Đăng nhập Google thất bại. Vui lòng thử lại."));
  }, [router]);

  return (
    <main className="flex flex-1 items-center justify-center px-4">
      <p className={error ? "text-red-600" : "text-zinc-500"}>{error ?? "Đang hoàn tất đăng nhập Google..."}</p>
    </main>
  );
}
