import { cn } from "@/lib/utils";

export function Container({
  className,
  ...props
}: React.ComponentProps<"div">) {
  return (
    <div
      className={cn("mx-auto flex w-full max-w-6xl flex-col gap-6 p-4 sm:p-6", className)}
      {...props}
    />
  );
}
