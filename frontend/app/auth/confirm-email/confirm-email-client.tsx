"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { CheckCircle2, Loader2, MailWarning } from "lucide-react";

import { Button } from "@/components/ui/button";
import { confirmEmail, resendConfirmation } from "@/lib/api";
import { getErrorMessage } from "@/lib/errors";

type Status = "idle" | "loading" | "success" | "error" | "missing";

export default function ConfirmEmailClient({
  email,
  token,
}: {
  email: string;
  token: string;
}) {
  const [status, setStatus] = useState<Status>("idle");
  const [message, setMessage] = useState("");
  const [resending, setResending] = useState(false);

  useEffect(() => {
    if (!email || !token) {
      setStatus("missing");
      return;
    }

    let cancelled = false;
    setStatus("loading");
    confirmEmail({ email, token })
      .then((result) => {
        if (cancelled) return;
        setStatus("success");
        setMessage(result.message);
      })
      .catch((error) => {
        if (cancelled) return;
        setStatus("error");
        setMessage(getErrorMessage(error, "Xác nhận email thất bại."));
      });

    return () => {
      cancelled = true;
    };
  }, [email, token]);

  async function handleResend() {
    if (!email) return;
    setResending(true);
    try {
      const result = await resendConfirmation({ email });
      setMessage(result.message);
    } catch (error) {
      setMessage(getErrorMessage(error, "Không gửi lại được email xác nhận."));
    } finally {
      setResending(false);
    }
  }

  return (
    <main className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center gap-4 px-4 text-center">
      {status === "loading" && (
        <>
          <Loader2 className="size-10 animate-spin text-primary" />
          <h1 className="text-xl font-semibold">Đang xác nhận email…</h1>
        </>
      )}

      {status === "success" && (
        <>
          <CheckCircle2 className="size-10 text-emerald-600" />
          <h1 className="text-xl font-semibold">Xác nhận thành công</h1>
          <p className="text-sm text-muted-foreground">
            {message || "Email của bạn đã được xác nhận."}
          </p>
          <Button asChild>
            <Link href="/auth/login">Đăng nhập</Link>
          </Button>
        </>
      )}

      {(status === "error" || status === "missing") && (
        <>
          <MailWarning className="size-10 text-amber-500" />
          <h1 className="text-xl font-semibold">
            {status === "missing"
              ? "Thiếu thông tin xác nhận"
              : "Không thể xác nhận email"}
          </h1>
          <p className="text-sm text-muted-foreground">
            {status === "missing"
              ? "Liên kết xác nhận không hợp lệ hoặc đã bị cắt bớt."
              : message}
          </p>
          {email && (
            <Button
              type="button"
              variant="outline"
              onClick={handleResend}
              disabled={resending}
            >
              {resending && <Loader2 className="animate-spin" />}
              Gửi lại email xác nhận
            </Button>
          )}
          <Button asChild variant="ghost">
            <Link href="/">Về trang chủ</Link>
          </Button>
        </>
      )}
    </main>
  );
}
