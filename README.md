# Playwright website tests

A small C# browser automation project using Playwright and NUnit. It opens a website in Chromium and checks that the response succeeds and the page has a title.

## Requirements

- .NET 10 SDK
- Windows PowerShell

## Run

From this folder:

```powershell
dotnet restore
powershell.exe -ExecutionPolicy Bypass -File .\bin\Debug\net10.0\playwright.ps1 install chromium
dotnet test
```

The browser-install command uses a process-scoped execution-policy bypass so it works when local PowerShell scripts are restricted. It does not change the machine's policy. The first run downloads Chromium; tests run headlessly by default.

By default, the sample tests `https://example.com`. Set `PLAYWRIGHT_BASE_URL` to point at your website:

```powershell
$env:PLAYWRIGHT_BASE_URL = "https://your-site.example"
dotnet test
```

Add tests in `Tests\WebsiteSmokeTests.cs`, using Playwright locators such as `Page.GetByRole(...)` to interact with your site's elements.
