using NUnit.Framework;
using Microsoft.Playwright;
using PlaywrightDemo.Pages;

namespace PlaywrightDemo;

public class UITestCasesFile
{
    [SetUp]
    public void Setup()
    {
        // No-op. Tests will create their own Playwright/browser instances.
    }

    [Test]
    public async Task UILoginSampleTestCase()
    {
        // Ensure Firefox browser is installed (best-effort)
        await PlaywrightBootstrap.EnsureInstalledAsync(5 * 60 * 1000, "firefox");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.Url("/sampleapp"));

        // Optionally increase timeout for slow environments
        page.SetDefaultTimeout(10000);

        // Use the existing page object helper if available
        var loginPages = new LoginPages(page);

        await page.ScreenshotAsync(new PageScreenshotOptions { Path = "screenshot.jpg" });

        await page.GetByRole(AriaRole.Button, new() { Name = "Log In" }).WaitForAsync();
        await page.FillAsync("input[name='UserName']", "admin");
        await page.FillAsync("input[name='Password']", "pwd");
        await page.ClickAsync("text=Log In");
        await page.Locator("#loginstatus").WaitForAsync();
        Assert.That(await page.Locator("#loginstatus").InnerTextAsync(), Is.EqualTo("Welcome, admin!"));

        await page.ClickAsync("text=Log Out");
        Assert.That(await page.Locator("#loginstatus").InnerTextAsync(), Is.EqualTo("User logged out."));

        await page.FillAsync("input[name='UserName']", "admin");
        await page.FillAsync("input[name='Password']", "pwdWrong");
        await page.ClickAsync("text=Log In");

        // Verify invalid login message
        await page.Locator("text=Invalid username/password").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        await browser.CloseAsync();
    }

    [Test]
    public async Task SelectDropDownValues()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/select");
        await page.SelectOptionAsync("#selectLanguage", new[] { "C#" });
        await page.SelectOptionAsync("#selectCity", new[] { "Las Vegas" });
        await page.SelectOptionAsync("#selectProduct", new[] { "Release 3.0 Beta" });
        await page.SelectOptionAsync("#selectColors", new[] { "Blue" });
        await page.SelectOptionAsync("#selectFruits", new[] { "Elderberry" });
        Thread.Sleep(5000); // Wait for 2 seconds to observe the selection

    }

    [Test]
    public async Task TextInputAndValidate()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/textinput");

        var randomText = RandomString(12);
        await page.FillAsync("#newButtonName", randomText);
        await page.ClickAsync("#updatingButton");
        await page.Locator("#updatingButton").WaitForAsync();
        await page.GetByText(randomText, new PageGetByTextOptions { Exact = true }).WaitForAsync();
        Assert.That(await page.Locator("#updatingButton").InnerTextAsync(), Is.EqualTo(randomText));      

        static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
            var rnd = new Random();
            var buf = new char[length];
            for (int i = 0; i < length; i++) buf[i] = chars[rnd.Next(chars.Length)];
            return new string(buf);
        }
    }
}

