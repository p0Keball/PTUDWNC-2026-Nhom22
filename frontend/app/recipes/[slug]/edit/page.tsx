import { redirect } from "next/navigation";

export default async function LegacyEditRecipePage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  redirect(`/dashboard/recipes/${encodeURIComponent(slug)}/edit`);
}
