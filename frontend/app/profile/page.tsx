"use client";

import { useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import Avatar from "@/components/Avatar";
import {
  ApiError,
  getMyProfile,
  resendConfirmation,
  updateMyProfile,
  type UserProfile,
} from "@/lib/api";
import { useAuthStore } from "@/store/auth-store";

export default function ProfilePage() {
  const router = useRouter();
  const accessToken = useAuthStore((state) => state.accessToken);
  const hasHydrated = useAuthStore((state) => state.hasHydrated);
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [displayName, setDisplayName] = useState("");
  const [avatarUrl, setAvatarUrl] = useState("");
  const [bio, setBio] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [resending, setResending] = useState(false);
  const [resendMessage, setResendMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!hasHydrated) return;
    if (!accessToken) {
      router.replace("/login");
      return;
    }
    getMyProfile(accessToken)
      .then((data) => {
        setProfile(data);
        setDisplayName(data.displayName);
        setAvatarUrl(data.avatarUrl ?? "");
        setBio(data.bio ?? "");
      })
      .catch((err) => {
        if (err instanceof ApiError && err.status === 401) {
          useAuthStore.getState().clearSession();
          router.replace("/login");
        } else setError("Không tải được hồ sơ.");
      })
      .finally(() => setLoading(false));
  }, [accessToken, hasHydrated, router]);

  const save = async (event: FormEvent) => {
    event.preventDefault();
    if (!accessToken) return router.replace("/login");
    setSaving(true);
    setError(null);
    setMessage(null);
    try {
      const updated = await updateMyProfile({ displayName, avatarUrl, bio }, accessToken);
      setProfile(updated);
      setMessage("Đã cập nhật hồ sơ.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Cập nhật hồ sơ thất bại.");
    } finally {
      setSaving(false);
    }
  };

  async function handleResendConfirmation() {
    if (!profile) return;
    setResending(true);
    setResendMessage(null);
    try {
      const result = await resendConfirmation({ email: profile.email });
      setResendMessage(result.message);
    } catch (err) {
      setResendMessage(
        err instanceof ApiError ? err.message : "Không gửi lại được email xác nhận."
      );
    } finally {
      setResending(false);
    }
  }

  if (loading) return <main className="mx-auto w-full max-w-3xl flex-1 p-6"><div className="h-64 animate-pulse rounded-3xl bg-zinc-100 dark:bg-zinc-900" /></main>;
  if (!profile) return <main className="flex flex-1 items-center justify-center p-6"><p className="text-red-600">{error ?? "Không tìm thấy hồ sơ."}</p></main>;

  return (
    <main className="mx-auto w-full max-w-3xl flex-1 px-4 py-8 sm:px-6">
      <Link href="/" className="text-sm font-medium text-orange-600 hover:underline">← Về trang chủ</Link>
      <div className="mt-5 overflow-hidden rounded-3xl bg-white shadow-xl dark:bg-zinc-900">
        <div className="h-32 bg-gradient-to-r from-orange-500 to-amber-300" />
        <div className="px-6 pb-8 sm:px-10">
          <div className="-mt-12 flex items-end justify-between">
            <Avatar name={profile.displayName} src={avatarUrl || null} />
            <span className="rounded-full bg-orange-100 px-3 py-1 text-sm font-semibold text-orange-700">{profile.role}</span>
          </div>
          <h1 className="mt-4 text-3xl font-bold">Hồ sơ cá nhân</h1>
          <p className="mt-1 flex flex-wrap items-center gap-2 text-sm text-zinc-500">
            {profile.email}
            {profile.emailConfirmed ? (
              <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-medium text-emerald-700">
                Đã xác nhận
              </span>
            ) : (
              <span className="rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-700">
                Chưa xác nhận email
              </span>
            )}
          </p>
          {!profile.emailConfirmed && (
            <div className="mt-3 rounded-xl bg-amber-50 p-3 text-sm text-amber-800">
              <p>Vui lòng xác nhận email để mở khóa đầy đủ quyền tác giả.</p>
              <button
                type="button"
                onClick={handleResendConfirmation}
                disabled={resending}
                className="mt-2 font-semibold underline disabled:opacity-60"
              >
                {resending ? "Đang gửi..." : "Gửi lại email xác nhận"}
              </button>
              {resendMessage && <p className="mt-1 text-xs">{resendMessage}</p>}
            </div>
          )}
          <form onSubmit={save} className="mt-8 space-y-5">
            <label className="block text-sm font-medium">Tên hiển thị<input value={displayName} onChange={(e) => setDisplayName(e.target.value)} required minLength={2} maxLength={100} className="mt-1 w-full rounded-xl border border-zinc-200 px-4 py-3 outline-none focus:border-orange-500 dark:border-zinc-700 dark:bg-zinc-800" /></label>
            <label className="block text-sm font-medium">Avatar URL<input type="url" value={avatarUrl} onChange={(e) => setAvatarUrl(e.target.value)} placeholder="https://..." className="mt-1 w-full rounded-xl border border-zinc-200 px-4 py-3 outline-none focus:border-orange-500 dark:border-zinc-700 dark:bg-zinc-800" /></label>
            <label className="block text-sm font-medium">Giới thiệu<textarea value={bio} onChange={(e) => setBio(e.target.value)} maxLength={5000} rows={4} className="mt-1 w-full resize-none rounded-xl border border-zinc-200 px-4 py-3 outline-none focus:border-orange-500 dark:border-zinc-700 dark:bg-zinc-800" /></label>
            {error && <p role="alert" className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{error}</p>}
            {message && <p role="status" className="rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700">{message}</p>}
            <button disabled={saving} className="rounded-xl bg-orange-500 px-6 py-3 font-semibold text-white hover:bg-orange-600 disabled:opacity-60">{saving ? "Đang lưu..." : "Lưu thay đổi"}</button>
          </form>
        </div>
      </div>
    </main>
  );
}
