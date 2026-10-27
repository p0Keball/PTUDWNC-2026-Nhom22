"use client";

import { useState, type FormEvent } from "react";
import Link from "next/link";
import { toast } from "sonner";
import { forgotPassword } from "@/lib/api";
import { Container } from "@/components/Container";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  async function submit(event: FormEvent) {
    event.preventDefault(); setLoading(true);
    try { await forgotPassword(email.trim()); toast.success("Nếu email tồn tại, hướng dẫn đã được gửi."); }
    catch { toast.error("Không thể gửi yêu cầu. Vui lòng thử lại."); }
    finally { setLoading(false); }
  }
  return <Container className="max-w-md justify-center py-10"><div className="rounded-xl border bg-card p-6"><h1 className="text-2xl font-bold">Quên mật khẩu</h1><p className="mt-2 text-sm text-muted-foreground">Nhập email để nhận liên kết đặt lại mật khẩu.</p><form onSubmit={submit} className="mt-6 space-y-4"><input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} className="w-full rounded-md border px-3 py-2" placeholder="you@example.com" /><button disabled={loading} className="w-full rounded-md bg-primary px-4 py-2 text-primary-foreground">{loading ? "Đang gửi..." : "Gửi liên kết"}</button></form><Link href="/auth/login" className="mt-4 block text-sm text-primary hover:underline">Quay lại đăng nhập</Link></div></Container>;
}
