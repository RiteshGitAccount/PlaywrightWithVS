using NUnit.Framework;
using Microsoft.Playwright.NUnit;
using System.Threading.Tasks;


namespace PlaywrightDemo;
public class NUnitPlaywright:PageTest
{
    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync("http://uitestingplayground.com/sampleapp");
       

    }

    [Test]
    public async Task LoginPage()
    {
       
        
        await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "screenshot.jpg"
                
            }
        );

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Log In" })).ToBeVisibleAsync();

    }
}
