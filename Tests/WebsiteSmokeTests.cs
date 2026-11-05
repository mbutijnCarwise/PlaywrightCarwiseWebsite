using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.Json;

namespace PlaywrightWebsiteTests.Tests;

[Parallelizable(ParallelScope.Self)]
public class WebsiteSmokeTests : PageTest
{
    private static string BaseUrl => Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "https://carwisegroup.com/";
    private sealed record ContactFormData(string FirstName, string LastName, string Email, int NumberVehicles, string[] Solutions);

    public override Task<BrowserTypeLaunchOptions?> LaunchOptionsAsync()
    {
        return Task.FromResult<BrowserTypeLaunchOptions?>(new BrowserTypeLaunchOptions
        {
            Headless = false // Set to true for headless mode, false for headed mode
        });
    }

    private static IEnumerable<TestCaseData> GetContactFormData()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "resources", "testData.json");
        
        List<ContactFormData> testCases = JsonSerializer.Deserialize<List<ContactFormData>>(
            File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException($"Failed to deserialize test data from {path}.");

        foreach (ContactFormData testCase in testCases)
            yield return new TestCaseData(testCase.FirstName, testCase.LastName, testCase.Email, testCase.NumberVehicles, testCase.Solutions)
                .SetName($"ContactFormSubmission_{testCase.FirstName}_{testCase.LastName}");
    }

    [SetUp] // Setup method to initialize the browser and page before all tests
    public async Task Setup()
    {
        await Page.GotoAsync(BaseUrl);
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

        if (Page is null)
            throw new InvalidOperationException("Page is not initialized.");
        ILocator contactHeader = Page.GetByRole(AriaRole.Heading, new() { Name = "Neem contact op met CarWise Group" });
        Assert.That(await contactHeader.TextContentAsync(), Is.Not.Empty, "The contact header should not be empty.");

        // Test the Carano page
        ILocator caranoLink = Page.Locator("#main a[href$='/bedrijven/carano/']");
        await caranoLink.ScrollIntoViewIfNeededAsync();
        await caranoLink.ClickAsync();

        ILocator caranoHeader = Page.GetByRole(AriaRole.Heading, new() { Name = "Over Carano" });
        await caranoHeader.WaitForAsync();
        Assert.That(await caranoHeader.TextContentAsync(), Does.Contain("Over Carano"));
    }

    [TestCaseSource(nameof(GetContactFormData))] // Use the test data from the JSON file
    public async Task ContactFormSubmission(string firstName, string lastName, string email, int numberVehicles, string[] solutions)
    {
        Console.WriteLine("Test for textfields on the contactpage");

        await AcceptCookieBannerIfPresent();
        await ClickOnContactLink();

        // Fill in the textfields on the contact page
        if (Page is null)
            throw new InvalidOperationException("Page is not initialized.");

        List<RequestInputField> inputFields =
        [
            new(Page, "Voornaam", firstName),
            new(Page, "Achternaam", lastName),
            new(Page, "Werk-e-mailadres", email),
            new(Page, "Hoeveel voertuigen telt uw wagenpark?", numberVehicles.ToString())
        ];

        foreach (RequestInputField inputField in inputFields)
        {
            await inputField.FillAsync();
        }

        // Select the solutions checkboxes based on the test data
        foreach (string solution in solutions)
        {
            ILocator solutionCheckbox = Page.GetByLabel(solution) ?? throw new Exception($"Checkbox for solution '{solution}' could not be found.");
            await solutionCheckbox.CheckAsync();
        }
    }

    public async Task AcceptCookieBannerIfPresent()
    {
        if (Page is null)
            return;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Accepteren" }).ClickAsync();
    }

    public async Task ClickOnContactLink()
    {
        if (Page is null)
            return;
        await Page.Locator("#main").GetByRole(AriaRole.Link, new() { Name = "Neem contact op", Exact = true }).ClickAsync();
    }

}