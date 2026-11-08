"use client";

import { useEffect } from "react";

import { setAccessTokenProvider } from "@/lib/auth-token";
import { useAuthStore } from "@/store/auth-store";

/**
 * Rehydrate auth store sau khi mount để tránh hydration mismatch (SSR).
 * Đồng thời migrate token cũ từ các key localStorage trước đây.
 */
export function AuthProvider() {
  useEffect(() => {
    // Nối API client với store: mọi request dùng seam getAccessToken()
    // sẽ lấy JWT mới nhất sau login/refresh.
    setAccessTokenProvider(() => useAuthStore.getState().accessToken);

    void useAuthStore.persist.rehydrate();

    const legacyToken =
      window.localStorage.getItem("accessToken") ??
      window.localStorage.getItem("access_token");

    if (legacyToken && !useAuthStore.getState().accessToken) {
      useAuthStore.getState().setAccessToken(legacyToken);
    }

    const onStorage = (event: StorageEvent) => {
      if (event.key === "culinary-blog-auth") {
        void useAuthStore.persist.rehydrate();
      }
    };

    window.addEventListener("storage", onStorage);
    return () => {
      window.removeEventListener("storage", onStorage);
      setAccessTokenProvider(null);
    };
  }, []);

  return null;
}
