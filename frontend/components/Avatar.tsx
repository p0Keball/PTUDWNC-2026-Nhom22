interface AvatarProps {
  name: string;
  src?: string | null;
  size?: "sm" | "lg";
}

export default function Avatar({ name, src, size = "lg" }: AvatarProps) {
  const initials = name
    .trim()
    .split(/\s+/)
    .slice(-2)
    .map((part) => part[0]?.toUpperCase())
    .join("") || "?";
  const dimensions = size === "lg" ? "h-24 w-24 text-2xl" : "h-10 w-10 text-sm";

  return src ? (
    /* Remote avatar URLs are user-provided and may use any trusted host. */
    // eslint-disable-next-line @next/next/no-img-element
    <img
      src={src}
      alt={`Ảnh đại diện của ${name}`}
      className={`${dimensions} rounded-full object-cover ring-4 ring-white shadow-md dark:ring-zinc-900`}
    />
  ) : (
    <div
      aria-label={`Ảnh đại diện của ${name}`}
      className={`${dimensions} flex items-center justify-center rounded-full bg-orange-500 font-bold text-white ring-4 ring-white shadow-md dark:ring-zinc-900`}
    >
      {initials}
    </div>
  );
}
