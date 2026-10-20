"use client";

import { useEffect } from "react";

import { Container } from "@/components/Container";
import { ErrorState } from "@/components/ErrorState";

export default function ErrorPage({
  error,
  retry,
}: {
  error: Error & { digest?: string };
  retry: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <Container>
      <ErrorState
        title="Trang gặp sự cố"
        message={
          error.message ||
          "Không thể tải nội dung. Vui lòng thử lại hoặc quay về trang chủ."
        }
        onRetry={retry}
        homeHref="/"
      />
    </Container>
  );
}
