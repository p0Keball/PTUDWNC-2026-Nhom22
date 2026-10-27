"use client";

import { useSearchParams, useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { resetPassword } from "@/lib/api";
import { Container } from "@/components/Container";

export default function ResetPasswordPage() {
  const params = useSearchParams(); const router = useRouter();
  const email = params.get("email") ?? ""; const token = params.get("token") ?? "";
  const [password, setPassword] = useState(""); const [confirm, setConfirm] = useState(""); const [loading, setLoading] = useState(false);
  async function submit(event: FormEvent) {
    event.preventDefault();
    if (password !== confirm) { toast.error("Mật khẩu xác nhận không khớp."); return; }
    setLoading(true);
    try { await resetPassword(email, token, password); toast.success("Đặt lại mật khẩu thành công."); router.replace("/auth/login"); }
    catch { toast.error("Liên kết không hợp lệ hoặc đã hết hạn."); }
    finally { setLoading(false); }
  }
  return <Container className="max-w-md justify-center py-10"><div className="rounded-xl border bg-card p-6"><h1 className="text-2xl font-bold">Đặt lại mật khẩu</h1><form onSubmit={submit} className="mt-6 space-y-4"><input type="password" required minLength={8} value={password} onChange={(e) => setPassword(e.target.value)} className="w-full rounded-md border px-3 py-2" placeholder="Mật khẩu mới" /><input type="password" required minLength={8} value={confirm} onChange={(e) => setConfirm(e.target.value)} className="w-full rounded-md border px-3 py-2" placeholder="Xác nhận mật khẩu" /><button disabled={loading || !email || !token} className="w-full rounded-md bg-primary px-4 py-2 text-primary-foreground">{loading ? "Đang xử lý..." : "Đặt lại mật khẩu"}</button></form></div></Container>;
}
