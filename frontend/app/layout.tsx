import type { Metadata, Viewport } from "next";
import { Geist, Geist_Mono } from "next/font/google";

import SiteFooter from "@/components/SiteFooter";
import SiteHeader from "@/components/SiteHeader";
import { Providers } from "@/components/providers";
import { absoluteUrl } from "@/lib/utils";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  metadataBase: new URL(absoluteUrl()),
  title: {
    default: "Culinary Blog — Công thức nấu ăn",
    template: "%s | Culinary Blog",
  },
  description:
    "Khám phá và chia sẻ công thức nấu ăn từ nhiều nền ẩm thực. Blog Ẩm thực và Nấu ăn của Nhóm 22.",
  applicationName: "Culinary Blog",
  keywords: ["công thức", "nấu ăn", "ẩm thực", "blog", "recipe"],
  openGraph: {
    type: "website",
    locale: "vi_VN",
    siteName: "Culinary Blog",
    title: "Culinary Blog — Công thức nấu ăn",
    description:
      "Khám phá và chia sẻ công thức nấu ăn từ nhiều nền ẩm thực.",
    url: "/",
  },
  twitter: {
    card: "summary_large_image",
    title: "Culinary Blog — Công thức nấu ăn",
    description:
      "Khám phá và chia sẻ công thức nấu ăn từ nhiều nền ẩm thực.",
  },
  robots: {
    index: true,
    follow: true,
  },
};

export const viewport: Viewport = {
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#ffffff" },
    { media: "(prefers-color-scheme: dark)", color: "#09090b" },
  ],
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html
      lang="vi"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="flex min-h-full flex-col">
        <Providers>
          <a
            href="#main-content"
            className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded-md focus:bg-primary focus:px-4 focus:py-2 focus:text-sm focus:text-primary-foreground"
          >
            Bỏ qua điều hướng
          </a>
          <SiteHeader />
          <div id="main-content" className="flex flex-1 flex-col">
            {children}
          </div>
          <SiteFooter />
        </Providers>
      </body>
    </html>
  );
}
