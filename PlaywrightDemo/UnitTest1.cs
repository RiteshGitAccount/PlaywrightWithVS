using NUnit.Framework;
using Microsoft.Playwright.NUnit;
namespace PlaywrightDemo;
public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task FlipKartLoginPage()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        var page = await browser.NewPageAsync();
        await page.GotoAsync("http://uitestingplayground.com/sampleapp");
        await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "screenshot.jpg"
                
            }
        );
        await page.FillAsync("UserName", "Password");
        await page.FillAsync("Password", "Password");
    }
}
