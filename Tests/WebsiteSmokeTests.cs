using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.Json;

namespace PlaywrightWebsiteTests.Tests;

[Parallelizable(ParallelScope.Self)]
public class WebsiteSmokeTests : PageTest
{
    private IPage? _page;
    private static string BaseUrl => Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "https://carwisegroup.com/";
    private IBrowser? _browser;
    private IPlaywright? _playwright;
    private sealed record ContactFormData(string FirstName, string LastName, string Email);

    private static IEnumerable<TestCaseData> GetContactFormData()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "resources", "testData.json");
        
        List<ContactFormData> cases = JsonSerializer.Deserialize<List<ContactFormData>>(
            File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException($"Failed to deserialize test data from {path}.");

        foreach (ContactFormData testCase in cases)
            yield return new TestCaseData(testCase.FirstName, testCase.LastName, testCase.Email);
    }

    [SetUp] // Setup method to initialize the browser and page before all tests
    public async Task Setup()
    {
        Console.WriteLine($"starting browser");

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false // Set to true if you don't want to see the browser UI during the test
        });
        _page = await _browser.NewPageAsync();  
        await _page.GotoAsync(BaseUrl);
    }

    [Test] // Test to verify that the home page loads successfully
    public async Task HomePageLoadsSuccessfully()
    {
        IResponse? response = await Page.GotoAsync(BaseUrl);
        Console.WriteLine($"Response status: {response?.Status}");
        Assert.That(response, Is.Not.Null, "The page did not return a response.");
        Assert.That(response!.Status, Is.LessThan(400), $"Unexpected HTTP status: {response.Status}.");
        Assert.That(await Page.TitleAsync(), Is.Not.Empty, "The page title should not be empty.");
    }

    [Test] // Test to verify that navigating to the contact page works and the contact header is present
    public async Task ContactPageLoadsSuccessfully()
    {
        Console.WriteLine("Test for contactpage loads successfully");

        await AcceptCookieBannerIfPresent(); // Accept the cookie banner if it appears
        await ClickOnContactLink(); // Click on the "Neem contact op" link to navigate to the contact page

        if (_page is null)
            throw new InvalidOperationException("Page is not initialized.");
        ILocator contactHeader = _page.GetByRole(AriaRole.Heading, new() { Name = "Neem contact op met CarWise Group" });
        Assert.That(await contactHeader.TextContentAsync(), Is.Not.Empty, "The contact header should not be empty.");

        // Test the Carano page
        ILocator caranoLink = _page.GetByRole(AriaRole.Link, new() { Name = "Ontdek meer" }).First;
        await caranoLink.ScrollIntoViewIfNeededAsync();
        await caranoLink.ClickAsync();

        ILocator caranoHeader = _page.Locator("xpath=//*[@id=\"main\"]/section[1]/div/div/div/h1");
        // ILocator caranoHeader = page.GetByRole(AriaRole.Heading, new() { Name = "Neem contact op met CarWise Group" });

        var caranoHeaderContent = await caranoHeader.TextContentAsync();
        Assert.That(caranoHeaderContent, Is.Not.Empty, "The carano header should not be empty.");
        Assert.That(caranoHeaderContent, Does.Contain("Over Carano"), "The carano header should be 'OverCarano'.");
    }

    [TestCaseSource(nameof(GetContactFormData))] // Use the test data from the JSON file
    public async Task ContactFormSubmission(string firstName, string lastName, string email)
    {
        Console.WriteLine("Test for textfields on the contactpage");

        await AcceptCookieBannerIfPresent();
        await ClickOnContactLink();

        // Fill in the textfields on the contact page
        if (_page is null)
            throw new InvalidOperationException("Page is not initialized.");
        ILocator firstNameField = _page.GetByLabel("Voornaam");
        ILocator lastNameField = _page.GetByLabel("Achternaam");
        ILocator emailField = _page.GetByLabel("Werk-e-mailadres");

        if (firstNameField is null || lastNameField is null || emailField is null) 
            throw new Exception("One or more textfields could not be found on the contact page.");

        await firstNameField.ScrollIntoViewIfNeededAsync();
        await firstNameField.FillAsync(firstName);
        await lastNameField.FillAsync(lastName);
        await emailField.FillAsync(email);
    }

    public async Task AcceptCookieBannerIfPresent()
    {
        if (_page is null)
            return;
        ILocator acceptButton = _page.GetByRole(AriaRole.Button, new() { Name = "Accepteren" });
        //if (await acceptButton.IsVisibleAsync())
            await acceptButton.ClickAsync();
    }

    public async Task ClickOnContactLink()
    {
        if (_page is null)
            return;
        ILocator contactLink = _page.Locator("xpath=//*[@id=\"main\"]/section[1]/div/div/div/a[1]/div/div[1]/span");
        //ILocator contactLink = _page.GetByLabel("Neem contact op");
        //ILocator contactLink = _page.GetByRole(AriaRole.Button, new() { Name = "Neem contact op" });
        await contactLink.ClickAsync();
    }

    [TearDown] // Teardown method to close the browser after all tests
    public async Task Teardown()    
    {
        Console.WriteLine($"closing browser");

        try {
            if (_browser is not null)
                await _browser.CloseAsync();
        }
        finally
        {
            _playwright?.Dispose();
        }
    }

}