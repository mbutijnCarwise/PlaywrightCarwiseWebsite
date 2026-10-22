using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightWebsiteTests.Tests;

[Parallelizable(ParallelScope.Self)]
public class WebsiteSmokeTests : PageTest
{
    //private static IPage page;
    private static string BaseUrl => Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "https://carwisegroup.com/";

    [Test] // Test to verify that the home page loads successfully
    public async Task HomePageLoadsSuccessfully()
    {
        var response = await Page.GotoAsync(BaseUrl);

        Console.WriteLine($"Response status: {response?.Status}");
        Assert.That(response, Is.Not.Null, "The page did not return a response.");
        Assert.That(response!.Status, Is.LessThan(400), $"Unexpected HTTP status: {response.Status}.");
        Assert.That(await Page.TitleAsync(), Is.Not.Empty, "The page title should not be empty.");
    }

    [Test] // Test to verify that navigating to the contact page works and the contact header is present
    public async Task ContactPageLoadsSuccessfully()
    {
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false // Set to false if you want to see the browser UI during the test
        });
        IPage page = await browser.NewPageAsync();
        await page.GotoAsync(BaseUrl);

        // Accept the cookie banner if it appears
        ILocator accept = page.Locator("xpath=//*[@id=\"cmplz-cookiebanner-1-optin\"]/div[6]/button[1]");
        await accept.ClickAsync();

        // Click on the "Neem contact op" link to navigate to the contact page
        ILocator contactLink = page.Locator("xpath=//*[@id=\"main\"]/section[1]/div/div/div/a[1]/div/div[1]/span");
        await contactLink.ClickAsync();

        ILocator contactHeader = page.GetByRole(AriaRole.Heading, new() { Name = "Neem contact op met CarWise Group" });
        Assert.That(await contactHeader.TextContentAsync(), Is.Not.Empty, "The contact header should not be empty.");

        // Test the Carano page
        // await page.GetByText("Ontdek meer").ClickAsync();
        ILocator caranoLink = page.Locator("xpath=//*[@id=\"main\"]/section[2]/div/div/div[1]/div/div[1]/div[3]/a/div/div[1]");
        await caranoLink.ScrollIntoViewIfNeededAsync();
        await caranoLink.ClickAsync();

        ILocator caranoHeader = page.Locator("xpath=//*[@id=\"main\"]/section[1]/div/div/div/h1");
        // ILocator caranoHeader = page.GetByRole(AriaRole.Heading, new() { Name = "Neem contact op met CarWise Group" });

        Assert.That(await caranoHeader.TextContentAsync(), Is.Not.Empty, "The carano header should not be empty.");
        Assert.That(await caranoHeader.TextContentAsync(), Does.Contain("Over Carano"), "The carano header should be 'OverCarano'.");
    }

}