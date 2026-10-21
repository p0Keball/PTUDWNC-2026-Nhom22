/**
 * Seam tạm thời giữa API client và auth-store.
 *
 * Hiện tại: đọc token từ localStorage (luồng cũ) để không block tính năng.
 * Khi `auth-store.ts` của Long hoàn thành, chỉ cần gọi:
 *   setAccessTokenProvider(() => useAuthStore.getState().accessToken);
 * Không phải sửa lại các hàm trong `lib/api.ts`.
 */
type AccessTokenProvider = () => string | null | undefined;

let provider: AccessTokenProvider | null = null;

export function setAccessTokenProvider(next: AccessTokenProvider | null): void {
  provider = next;
}

export function getAccessToken(): string | undefined {
  // Có store thì tin store tuyệt đối (kể cả null sau khi logout).
  if (provider) return provider() ?? undefined;

  // Fallback luồng cũ — chỉ chạy ở browser.
  if (typeof window === "undefined") return undefined;
  return (
    window.localStorage.getItem("accessToken") ??
    window.localStorage.getItem("access_token") ??
    undefined
  );
}
