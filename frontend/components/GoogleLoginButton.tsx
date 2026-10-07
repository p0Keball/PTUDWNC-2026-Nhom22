"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { loginWithGoogleCode } from "@/lib/api";

const verifierKey = "foodblog_google_pkce_verifier";
const stateKey = "foodblog_google_oauth_state";

function toBase64Url(bytes: Uint8Array) {
  let binary = "";
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte);
  });
  return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

async function createChallenge(verifier: string) {
  const encoded = new TextEncoder().encode(verifier);
  const digest = await crypto.subtle.digest("SHA-256", encoded);
  return toBase64Url(new Uint8Array(digest));
}

function createRandomValue() {
  return toBase64Url(crypto.getRandomValues(new Uint8Array(32)));
}

export default function GoogleLoginButton() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const code = params.get("code");
    const returnedState = params.get("state");
    const expectedState = sessionStorage.getItem(stateKey);
    const verifier = sessionStorage.getItem(verifierKey);

    if (!code && !params.get("error")) return;
    window.history.replaceState({}, "", "/login");

    if (params.get("error")) {
      window.setTimeout(() => setError("Bạn đã hủy đăng nhập Google."), 0);
      return;
    }

    if (!code || !returnedState || returnedState !== expectedState || !verifier) {
      window.setTimeout(
        () => setError("Phiên đăng nhập Google không hợp lệ hoặc đã hết hạn."),
        0
      );
      return;
    }

    window.setTimeout(() => {
      setLoading(true);
      loginWithGoogleCode(code, verifier)
        .then((result) => {
          localStorage.setItem("foodblog_refresh_token", result.refreshToken);
          localStorage.setItem("foodblog_user", JSON.stringify(result.user));
          sessionStorage.removeItem(stateKey);
          sessionStorage.removeItem(verifierKey);
          router.push("/");
        })
        .catch((reason: Error) => setError(reason.message))
        .finally(() => setLoading(false));
    }, 0);
  }, [router]);

  async function startLogin() {
    setError(null);
    setLoading(true);

    try {
      const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;
      const redirectUri = `${window.location.origin}/login`;
      if (!clientId) throw new Error("Chưa cấu hình Google Client ID.");

      const verifier = createRandomValue();
      const state = createRandomValue();
      sessionStorage.setItem(verifierKey, verifier);
      sessionStorage.setItem(stateKey, state);
      const challenge = await createChallenge(verifier);
      const query = new URLSearchParams({
        client_id: clientId,
        redirect_uri: redirectUri,
        response_type: "code",
        scope: "openid email profile",
        code_challenge: challenge,
        code_challenge_method: "S256",
        state,
      });
      window.location.assign(`https://accounts.google.com/o/oauth2/v2/auth?${query}`);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Không thể bắt đầu đăng nhập Google.");
      setLoading(false);
    }
  }

  return (
    <div className="w-full max-w-sm space-y-4">
      <button
        type="button"
        onClick={startLogin}
        disabled={loading}
        className="flex w-full items-center justify-center gap-3 rounded-xl border border-zinc-300 bg-white px-5 py-3 font-semibold text-zinc-800 shadow-sm transition hover:border-zinc-500 hover:shadow disabled:cursor-wait disabled:opacity-60"
      >
        <span className="text-lg font-bold text-[#4285f4]">G</span>
        {loading ? "Đang kết nối Google..." : "Tiếp tục với Google"}
      </button>
      {error && <p className="text-center text-sm text-red-600">{error}</p>}
    </div>
  );
}