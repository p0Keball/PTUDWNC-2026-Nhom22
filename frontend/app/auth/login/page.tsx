"use client";

import { useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ChefHat, LogIn } from "lucide-react";
import { toast } from "sonner";

import { Container } from "@/components/Container";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getErrorMessage } from "@/lib/errors";
import { useAuthStore } from "@/store/auth-store";

export default function LoginPage() {
  const router = useRouter();
  const login = useAuthStore((state) => state.login);
  const user = useAuthStore((state) => state.user);
  const hasHydrated = useAuthStore((state) => state.hasHydrated);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (hasHydrated && user) router.replace("/dashboard");
  }, [hasHydrated, user, router]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await login(email.trim(), password);
      toast.success("Đăng nhập thành công.");
      router.replace("/dashboard");
    } catch (err) {
      setError(getErrorMessage(err, "Đăng nhập thất bại."));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <Container className="max-w-md justify-center py-10">
      <Card>
        <CardHeader className="items-center text-center">
          <div className="mb-2 flex size-12 items-center justify-center rounded-full bg-primary/10 text-primary">
            <ChefHat />
          </div>
          <CardTitle>Đăng nhập</CardTitle>
          <CardDescription>
            Đăng nhập để quản lý công thức và sử dụng dashboard.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            {error && (
              <Alert variant="destructive">
                <AlertTitle>Không thể đăng nhập</AlertTitle>
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                placeholder="you@example.com"
                required
                disabled={submitting}
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="password">Mật khẩu</Label>
              <Input
                id="password"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                required
                disabled={submitting}
              />
            </div>

            <Button type="submit" disabled={submitting}>
              <LogIn />
              {submitting ? "Đang đăng nhập…" : "Đăng nhập"}
            </Button>

            <p className="text-center text-sm text-muted-foreground">
              Chưa có tài khoản?{" "}
              <Link href="/auth/register" className="font-medium text-primary underline-offset-4 hover:underline">
                Đăng ký
              </Link>
            </p>
            <Link href="/auth/forgot-password" className="text-center text-sm text-primary hover:underline">
              Quên mật khẩu?
            </Link>
          </form>
        </CardContent>
      </Card>
    </Container>
  );
}
