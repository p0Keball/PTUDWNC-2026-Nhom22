"use client";

import { useState, type FormEvent } from "react";
import Link from "next/link";
import { Lock, ArrowLeft } from "lucide-react";
import { toast } from "sonner";

import { PageHeader } from "@/components/PageHeader";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { changePassword } from "@/lib/api";
import { getErrorMessage } from "@/lib/errors";
import { useAuthStore } from "@/store/auth-store";

export default function ChangePasswordPage() {
  const token = useAuthStore((state) => state.accessToken);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!token) return;

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
      await changePassword(
        { currentPassword, newPassword },
        token
      );
      toast.success("Đã đổi mật khẩu thành công.");
    } catch (err) {
      toast.error(getErrorMessage(err, "Đổi mật khẩu thất bại."));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-[calc(100vh-4rem)] flex-col items-center justify-center bg-muted/50 px-4">
      <PageHeader
        title="Đổi mật khẩu"
        description="Cập nhật mật khẩu mới cho tài khoản của bạn."
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
          <Label htmlFor="currentPassword">Mật khẩu hiện tại</Label>
          <Input
            id="currentPassword"
            type="password"
            required
            minLength={8}
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
            placeholder="••••••••"
            className="w-full"
          />
        </div>

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
          {loading ? "Đang lưu…" : "Đổi mật khẩu"}
        </Button>
      </form>
    </div>
  );
}
