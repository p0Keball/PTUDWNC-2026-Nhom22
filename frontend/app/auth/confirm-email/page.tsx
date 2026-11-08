import type { Metadata } from "next";

import ConfirmEmailClient from "./confirm-email-client";

export const metadata: Metadata = {
  title: "Xác nhận email",
  description: "Xác nhận địa chỉ email cho tài khoản FoodBlog.",
  robots: { index: false, follow: false },
};

interface ConfirmEmailPageProps {
  searchParams: Promise<{ email?: string; token?: string }>;
}

export default async function ConfirmEmailPage({
  searchParams,
}: ConfirmEmailPageProps) {
  const { email = "", token = "" } = await searchParams;
  return <ConfirmEmailClient email={email} token={token} />;
}
