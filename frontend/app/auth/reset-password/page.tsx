"use client";

import { useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Lock, ArrowLeft } from "lucide-react";
import { toast } from "sonner";

import { PageHeader } from "@/components/PageHeader";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { resetPassword } from "@/lib/api";
import { getErrorMessage } from "@/lib/errors";

export default function ResetPasswordPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const email = searchParams.get("email") ?? "";
  const token = searchParams.get("token") ?? "";

  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!email || !token) {
      toast.error("Liên kết khôi phục không hợp lệ.");
      router.push("/auth/login");
    }
  }, [email, token, router]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();

    if (newPassword !== confirmPassword) {
      toast.error("Mật khẩu mới và xác nhận mật khẩu không khớp.");
      return;
    }
    if (newPassword.length < 8) {
      toast.error("Mật khẩu mới phải có ít nhất 8 ký tự.");
      return;
    }

    setLoading(true);
    try {
      await resetPassword({ email, token, newPassword });
      toast.success("Đã đặt lại mật khẩu thành công. Bạn có thể đăng nhập ngay.");
      router.push("/auth/login");
    } catch (err) {
      toast.error(getErrorMessage(err, "Đặt lại mật khẩu thất bại."));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-[calc(100vh-4rem)] flex-col items-center justify-center bg-muted/50 px-4">
      <PageHeader
        title="Đặt lại mật khẩu"
        description="Tạo mật khẩu mới cho tài khoản của bạn."
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
          <Label htmlFor="newPassword">Mật khẩu mới</Label>
          <Input
            id="newPassword"
            type="password"
            required
            minLength={8}
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            placeholder="••••••••"
            className="w-full"
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="confirmPassword">Xác nhận mật khẩu mới</Label>
          <Input
            id="confirmPassword"
            type="password"
            required
            minLength={8}
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="••••••••"
            className="w-full"
          />
        </div>

        <Button
          type="submit"
          disabled={loading}
          className="w-full"
        >
          <Lock className="size-4 mr-2" />
          {loading ? "Đang lưu…" : "Đặt lại mật khẩu"}
        </Button>
      </form>
    </div>
  );
}
