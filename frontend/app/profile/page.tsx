"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { profileApi, UserProfile } from "@/lib/api";

export default function ProfilePage() {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [displayName, setDisplayName] = useState("");
  const [avatarUrl, setAvatarUrl] = useState("");
  const [bio, setBio] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    profileApi.get()
      .then((data) => {
        setProfile(data);
        setDisplayName(data.displayName);
        setAvatarUrl(data.avatarUrl ?? "");
        setBio(data.bio ?? "");
      })
      .catch((reason: Error) => setError(reason.message))
      .finally(() => setLoading(false));
  }, []);

  async function saveProfile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setMessage(null);
    setError(null);
    try {
      const updated = await profileApi.update({ displayName, avatarUrl, bio });
      setProfile(updated);
      setDisplayName(updated.displayName);
      setAvatarUrl(updated.avatarUrl ?? "");
      setBio(updated.bio ?? "");
      setMessage("Hồ sơ đã được cập nhật.");
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Cập nhật hồ sơ thất bại.");
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <main className="mx-auto w-full max-w-2xl p-6 text-zinc-500">Đang tải hồ sơ...</main>;
  }

  if (!profile) {
    return (
      <main className="mx-auto w-full max-w-2xl space-y-4 p-6">
        <p className="text-red-600">{error ?? "Bạn cần đăng nhập để xem hồ sơ."}</p>
        <Link href="/login" className="font-semibold underline">Đến trang đăng nhập</Link>
      </main>
    );
  }

  return (
    <main className="mx-auto w-full max-w-2xl space-y-8 p-6 sm:p-10">
      <div>
        <Link href="/" className="text-sm font-semibold text-zinc-500 hover:text-zinc-950">Culinary Blog</Link>
        <h1 className="mt-8 text-3xl font-bold tracking-tight">Hồ sơ cá nhân</h1>
        <p className="mt-2 text-sm text-zinc-500">Quản lý thông tin hiển thị trên tài khoản của bạn.</p>
      </div>

      <form onSubmit={saveProfile} className="space-y-5 rounded-2xl border border-zinc-200 bg-white p-6 shadow-sm">
        <div>
          <label htmlFor="email" className="mb-2 block text-sm font-semibold">Email</label>
          <input id="email" value={profile.email} disabled className="w-full rounded-lg border border-zinc-200 bg-zinc-100 px-3 py-2.5 text-zinc-500" />
        </div>
        <div>
          <label htmlFor="displayName" className="mb-2 block text-sm font-semibold">Tên hiển thị</label>
          <input id="displayName" value={displayName} onChange={(event) => setDisplayName(event.target.value)} minLength={2} maxLength={100} required className="w-full rounded-lg border border-zinc-300 px-3 py-2.5 outline-none focus:border-zinc-900" />
        </div>
        <div>
          <label htmlFor="avatarUrl" className="mb-2 block text-sm font-semibold">URL ảnh đại diện</label>
          <input id="avatarUrl" type="url" value={avatarUrl} onChange={(event) => setAvatarUrl(event.target.value)} maxLength={500} placeholder="https://..." className="w-full rounded-lg border border-zinc-300 px-3 py-2.5 outline-none focus:border-zinc-900" />
        </div>
        <div>
          <label htmlFor="bio" className="mb-2 block text-sm font-semibold">Giới thiệu</label>
          <textarea id="bio" value={bio} onChange={(event) => setBio(event.target.value)} maxLength={1000} rows={5} className="w-full resize-y rounded-lg border border-zinc-300 px-3 py-2.5 outline-none focus:border-zinc-900" />
          <p className="mt-1 text-right text-xs text-zinc-400">{bio.length}/1000</p>
        </div>
        {error && <p className="text-sm text-red-600">{error}</p>}
        {message && <p className="text-sm text-emerald-700">{message}</p>}
        <button type="submit" disabled={saving} className="rounded-lg bg-zinc-950 px-5 py-3 font-semibold text-white transition hover:bg-zinc-700 disabled:cursor-wait disabled:opacity-60">
          {saving ? "Đang lưu..." : "Lưu thay đổi"}
        </button>
      </form>
    </main>
  );
}