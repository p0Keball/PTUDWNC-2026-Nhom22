import * as React from "react";

import { cn } from "@/lib/utils";

function Select({ className, children, ...props }: React.ComponentProps<"select">) {
  return (
    <select
      data-slot="select"
      className={cn(
        "flex h-9 w-full min-w-0 rounded-md border border-input bg-card text-foreground px-3 py-1 text-sm shadow-xs outline-none transition-[color,box-shadow] cursor-pointer disabled:cursor-not-allowed disabled:opacity-50",
        "focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50",
        "aria-invalid:border-destructive aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40",
        "dark:bg-zinc-900 dark:text-zinc-100 dark:border-zinc-800",
        "[&_option]:bg-white [&_option]:text-zinc-900 dark:[&_option]:bg-zinc-900 dark:[&_option]:text-zinc-100",
        className
      )}
      {...props}
    >
      {children}
    </select>
  );
}

export { Select };
