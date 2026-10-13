const apiUrl = process.env.NEXT_PUBLIC_API_URL?.trim();

export const appConfig = {
  apiUrl: (apiUrl || "http://localhost:5000").replace(/\/+$/, ""),
  googleClientId: process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID?.trim() || "",
} as const;
