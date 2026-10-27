import Link from "next/link";
import { searchRecipes } from "@/lib/api";
import RecipeCard from "@/components/RecipeCard";
import { Container } from "@/components/Container";
import { PageHeader } from "@/components/PageHeader";

export const dynamic = "force-dynamic";
export const metadata = { title: "Tìm kiếm công thức" };

export default async function SearchPage({ searchParams }: { searchParams: Promise<{ q?: string }> }) {
  const q = (await searchParams).q?.trim() ?? "";
  const result = q ? await searchRecipes({ q }) : null;
  return (
    <Container>
      <PageHeader title="Tìm kiếm công thức" description="Tìm món ăn theo tên, mô tả hoặc nguyên liệu." />
      <form className="flex gap-2" action="/search">
        <input name="q" defaultValue={q} placeholder="Ví dụ: phở, bánh mì..." className="min-w-0 flex-1 rounded-md border bg-background px-3 py-2" />
        <button className="rounded-md bg-primary px-4 py-2 text-primary-foreground">Tìm kiếm</button>
      </form>
      {!q ? <p className="py-10 text-center text-muted-foreground">Nhập từ khóa để bắt đầu.</p> : result && result.items.length === 0 ? (
        <p className="py-10 text-center text-muted-foreground">Không tìm thấy công thức phù hợp.</p>
      ) : result ? (
        <div className="grid gap-4 pt-6 sm:grid-cols-2 lg:grid-cols-3">{result.items.map((recipe) => <RecipeCard key={recipe.id} recipe={recipe} />)}</div>
      ) : null}
      <Link href="/recipes" className="mt-6 inline-block text-sm text-primary hover:underline">Xem tất cả công thức</Link>
    </Container>
  );
}
