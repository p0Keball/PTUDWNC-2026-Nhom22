import { redirect } from "next/navigation";

export default function LegacyCreateRecipePage() {
  redirect("/dashboard/recipes/new");
}
