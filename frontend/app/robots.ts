import type { MetadataRoute } from "next";

/**
 * NFR-SEO-003: robots.txt chuẩn SEO. Chặn các khu vực cần đăng nhập
 * (dashboard, admin, profile) khỏi crawler.
 */
const SITE_URL = (
  process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000"
).replace(/\/+$/, "");

export default function robots(): MetadataRoute.Robots {
  return {
    rules: {
      userAgent: "*",
      allow: "/",
      disallow: ["/dashboard/", "/admin/", "/profile/", "/auth/"],
    },
    sitemap: `${SITE_URL}/sitemap.xml`,
    host: SITE_URL,
  };
}
