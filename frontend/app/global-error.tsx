"use client";

import { useEffect } from "react";

export default function GlobalError({
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
    <html lang="vi">
      <body
        style={{
          margin: 0,
          minHeight: "100vh",
          display: "grid",
          placeItems: "center",
          fontFamily: "ui-sans-serif, system-ui, sans-serif",
          background: "#09090b",
          color: "#fafafa",
        }}
      >
        <div style={{ maxWidth: 420, padding: 24, textAlign: "center" }}>
          <h1 style={{ fontSize: 24, marginBottom: 8 }}>
            Ứng dụng gặp sự cố
          </h1>
          <p style={{ color: "#a1a1aa", marginBottom: 20 }}>
            Đã có lỗi không mong muốn xảy ra. Vui lòng thử tải lại trang.
          </p>
          <button
            type="button"
            onClick={() => retry()}
            style={{
              cursor: "pointer",
              borderRadius: 8,
              border: "none",
              padding: "10px 18px",
              background: "#fafafa",
              color: "#18181b",
              fontWeight: 600,
            }}
          >
            Thử lại
          </button>
        </div>
      </body>
    </html>
  );
}
