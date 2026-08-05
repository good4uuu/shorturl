import { expect, test } from "@playwright/test";

test("shortens a URL and shows actions", async ({ page }) => {
  await page.route("**/api/urls?limit=8", async (route) => {
    await route.fulfill({ json: [] });
  });
  await page.route("**/api/urls", async (route) => {
    await route.fulfill({
      status: 201,
      json: {
        originalUrl: "https://www.example.com/products/category/item?id=12345",
        shortCode: "abc123X",
        shortUrl: "https://sho.rt/abc123X",
      },
    });
  });

  await page.goto("/");
  await page
    .getByLabel("Your long URL")
    .fill("https://www.example.com/products/category/item?id=12345");
  await page.getByRole("button", { name: "Shorten URL" }).click();

  await expect(page.getByText("Your shortened URL is ready")).toBeVisible();
  await expect(
    page.getByRole("link", { name: "https://sho.rt/abc123X" }),
  ).toBeVisible();
  await expect(page.getByRole("button", { name: "Copy link" })).toBeVisible();
  await expect(page.getByRole("img", { name: /QR code/ })).toBeVisible();
});

test("shows an API validation error", async ({ page }) => {
  await page.route("**/api/urls?limit=8", async (route) => {
    await route.fulfill({ json: [] });
  });
  await page.route("**/api/urls", async (route) => {
    await route.fulfill({
      status: 400,
      json: { error: "Please enter a valid HTTP or HTTPS URL." },
    });
  });

  await page.goto("/");
  await page.getByLabel("Your long URL").fill("https://example.com/long-link");
  await page.getByRole("button", { name: "Shorten URL" }).click();

  await expect(
    page.getByText("Please enter a valid HTTP or HTTPS URL."),
  ).toBeVisible();
});
