using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightWebsiteTests.Tests;

public class RequestInputField(IPage page, string label, string value) : PageTest
{
    private IPage? _page = page;
    private string _label = label;
    private string _value = value;

    public async Task FillAsync()
    {
        if (_page is null)
            throw new InvalidOperationException("Page is not initialized.");

        ILocator inputField = _page.GetByLabel(_label);
        await inputField.ScrollIntoViewIfNeededAsync();    

        if (inputField is null)
            throw new Exception($"Input field for label '{_label}' could not be found.");

        await inputField.FillAsync(_value);    
    }

}