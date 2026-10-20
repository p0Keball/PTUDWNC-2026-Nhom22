"use client";

import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

import { getErrorMessage } from "@/lib/errors";
import { http } from "@/lib/http";

/* -------------------------------------------------------------------------- */
/* Types theo SRS FR-AUTH                                                     */
/* -------------------------------------------------------------------------- */

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  userName?: string;
  avatarUrl?: string | null;
  bio?: string | null;
  role: string;
  roles?: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: AuthUser;
}

interface RawAuthUser {
  id: string;
  email: string;
  fullName?: string;
  displayName?: string;
  userName?: string;
  avatarUrl?: string | null;
  bio?: string | null;
  role?: string;
  roles?: string[];
}

interface RawAuthResponse {
  accessToken?: string;
  refreshToken: string;
  expiresAt?: string;
  user: RawAuthUser;
}

interface ProfileResponse {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export type AuthStatus =
  | "idle"
  | "loading"
  | "authenticated"
  | "unauthenticated";

interface AuthState {
  user: AuthUser | null;
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: string | null;
  status: AuthStatus;
  hasHydrated: boolean;
  error: string | null;

  setHasHydrated: (value: boolean) => void;
  setSession: (response: AuthResponse) => void;
  setAccessToken: (token: string | null) => void;
  setUser: (user: AuthUser | null) => void;
  clearSession: () => void;

  login: (email: string, password: string) => Promise<AuthUser>;
  register: (input: {
    fullName: string;
    email: string;
    userName: string;
    password: string;
  }) => Promise<AuthUser>;
  refresh: () => Promise<string | null>;
  fetchMe: () => Promise<AuthUser | null>;
  logout: () => Promise<void>;
}

/* -------------------------------------------------------------------------- */
/* Normalizers — backend hiện trả về UserDto, SRS dùng fullName/roles          */
/* -------------------------------------------------------------------------- */

function normalizeUser(raw: RawAuthUser | ProfileResponse): AuthUser {
  const fullName =
    ("fullName" in raw ? raw.fullName : undefined) ??
    ("displayName" in raw ? raw.displayName : undefined) ??
    raw.email;
  const roles = "roles" in raw ? raw.roles : undefined;

  return {
    id: raw.id,
    email: raw.email,
    fullName,
    userName: "userName" in raw ? raw.userName : undefined,
    avatarUrl: raw.avatarUrl ?? null,
    bio: raw.bio ?? null,
    role: raw.role ?? roles?.[0] ?? "Author",
    roles,
  };
}

function normalizeAuthResponse(raw: RawAuthResponse): AuthResponse {
  return {
    // Cookie-auth hiện tại có thể trả accessToken rỗng; vẫn giữ để TV1 nối JWT sau.
    accessToken: raw.accessToken ?? "",
    refreshToken: raw.refreshToken,
    expiresAt: raw.expiresAt ?? new Date(Date.now() + 15 * 60_000).toISOString(),
    user: normalizeUser(raw.user),
  };
}

const AUTH_STORAGE_KEY = "culinary-blog-auth";

/* -------------------------------------------------------------------------- */
/* Store                                                                      */
/* -------------------------------------------------------------------------- */

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
      status: "idle",
      hasHydrated: false,
      error: null,

      setHasHydrated: (value) => set({ hasHydrated: value }),

      setSession: (response) =>
        set({
          user: response.user,
          accessToken: response.accessToken || null,
          refreshToken: response.refreshToken || null,
          expiresAt: response.expiresAt || null,
          status: "authenticated",
          error: null,
        }),

      setAccessToken: (token) =>
        set({
          accessToken: token,
          status: token || get().user ? "authenticated" : "unauthenticated",
        }),

      setUser: (user) =>
        set({
          user,
          status: user || get().accessToken ? "authenticated" : "unauthenticated",
        }),

      clearSession: () =>
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          expiresAt: null,
          status: "unauthenticated",
          error: null,
        }),

      login: async (email, password) => {
        set({ status: "loading", error: null });
        try {
          const raw = await http.post<RawAuthResponse>(
            "/api/v1/auth/login",
            { email, password },
            { fallbackMessage: "Đăng nhập thất bại." }
          );
          const response = normalizeAuthResponse(raw);
          set({
            user: response.user,
            accessToken: response.accessToken || null,
            refreshToken: response.refreshToken || null,
            expiresAt: response.expiresAt,
            status: "authenticated",
            error: null,
          });
          return response.user;
        } catch (error) {
          const message = getErrorMessage(error, "Đăng nhập thất bại.");
          set({ status: "unauthenticated", error: message });
          throw error;
        }
      },

      register: async (input) => {
        set({ status: "loading", error: null });
        try {
          const raw = await http.post<RawAuthResponse>(
            "/api/v1/auth/register",
            {
              // Backend hiện tại dùng DisplayName; gửi kèm fullName/userName
              // để tương thích khi TV1 chuyển sang contract của SRS.
              displayName: input.fullName,
              fullName: input.fullName,
              userName: input.userName,
              email: input.email,
              password: input.password,
            },
            { fallbackMessage: "Đăng ký thất bại." }
          );
          const response = normalizeAuthResponse(raw);
          set({
            user: response.user,
            accessToken: response.accessToken || null,
            refreshToken: response.refreshToken || null,
            expiresAt: response.expiresAt,
            status: "authenticated",
            error: null,
          });
          return response.user;
        } catch (error) {
          const message = getErrorMessage(error, "Đăng ký thất bại.");
          set({ status: "unauthenticated", error: message });
          throw error;
        }
      },

      refresh: async () => {
        const refreshToken = get().refreshToken;
        if (!refreshToken) return null;

        try {
          const raw = await http.post<RawAuthResponse>(
            "/api/v1/auth/refresh",
            { refreshToken },
            { fallbackMessage: "Phiên đăng nhập đã hết hạn." }
          );
          const response = normalizeAuthResponse(raw);
          set({
            user: response.user,
            accessToken: response.accessToken || null,
            refreshToken: response.refreshToken || null,
            expiresAt: response.expiresAt,
            status: "authenticated",
            error: null,
          });
          return response.accessToken || null;
        } catch (error) {
          set({
            user: null,
            accessToken: null,
            refreshToken: null,
            expiresAt: null,
            status: "unauthenticated",
            error: getErrorMessage(error, "Phiên đăng nhập đã hết hạn."),
          });
          return null;
        }
      },

      fetchMe: async () => {
        const token = get().accessToken;
        try {
          const profile = await http.get<ProfileResponse>("/api/v1/auth/me", {
            token,
            cache: "no-store",
            fallbackMessage: "Không tải được hồ sơ người dùng.",
          });
          const user = normalizeUser(profile);
          set({ user, status: "authenticated", error: null });
          return user;
        } catch {
          // Cookie-auth có thể vẫn hợp lệ dù chưa có accessToken; không xóa session ngay.
          if (token) get().clearSession();
          return null;
        }
      },

      logout: async () => {
        const { accessToken, refreshToken } = get();
        try {
          await http.post<void>(
            "/api/v1/auth/logout",
            refreshToken ? { refreshToken } : undefined,
            {
              token: accessToken,
              fallbackMessage: "Đăng xuất thất bại.",
            }
          );
        } catch {
          // Logout phải idempotent phía client: luôn xóa session local.
        } finally {
          get().clearSession();
        }
      },
    }),
    {
      name: AUTH_STORAGE_KEY,
      storage: createJSONStorage(() => localStorage),
      // Tránh hydration mismatch khi SSR; AuthProvider sẽ rehydrate trên client.
      skipHydration: true,
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        expiresAt: state.expiresAt,
      }),
      onRehydrateStorage: () => (state) => {
        if (!state) return;
        state.setHasHydrated(true);
        state.setAccessToken(state.accessToken);
      },
    }
  )
);

/** Dùng ngoài React (vd. trong callback) để lấy token hiện tại. */
export function getAccessToken() {
  return useAuthStore.getState().accessToken;
}

/** Selector tiện dụng cho component. */
export function useCurrentUser() {
  return useAuthStore((state) => state.user);
}
