"use client";

import { useState, type FormEvent } from "react";
import Link from "next/link";
import { Mail, ArrowLeft } from "lucide-react";
import { toast } from "sonner";

import { PageHeader } from "@/components/PageHeader";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { forgotPassword } from "@/lib/api";
import { getErrorMessage } from "@/lib/errors";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();

    setLoading(true);
    try {
      await forgotPassword({ email });
      toast.success(
        "Nếu email tồn tại trong hệ thống, liên kết khôi phục đã được gửi đến hộp thư của bạn."
      );
    } catch (err) {
      toast.error(getErrorMessage(err, "Gửi yêu cầu quên mật khẩu thất bại."));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-[calc(100vh-4rem)] flex-col items-center justify-center bg-muted/50 px-4">
      <PageHeader
        title="Quên mật khẩu"
        description="Nhập email của bạn để nhận liên kết đặt lại mật khẩu."
        actions={
          <Button asChild size="sm">
            <Link href="/auth/login">
              <ArrowLeft className="size-4 mr-1" />
              Đăng nhập
            </Link>
          </Button>
        }
      />

      <form
        onSubmit={handleSubmit}
        className="w-full max-w-md space-y-4"
      >
        <div className="space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="email@example.com"
            className="w-full"
          />
        </div>

        <Button
          type="submit"
          disabled={loading}
          className="w-full"
        >
          <Mail className="size-4 mr-2" />
          {loading ? "Đang gửi…" : "Gửi liên kết khôi phục"}
        </Button>
      </form>
    </div>
  );
}
