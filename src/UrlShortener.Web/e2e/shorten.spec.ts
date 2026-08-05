import { test, expect } from "@playwright/test";

test("shortening flow displays a result", async ({ page }) => {
  await page.goto("http://localhost:5173");
  await page
    .getByLabel("Your long URL")
    .fill("https://www.example.com/products/category/item?id=12345");
  await page.getByRole("button", { name: "Shorten URL" }).click();
  await expect(page.getByText("Your shortened URL is ready")).toBeVisible();
});
