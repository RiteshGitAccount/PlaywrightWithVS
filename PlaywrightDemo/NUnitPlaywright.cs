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
        await Page.FillAsync("input[name='UserName']", "admin");   
        await Page.FillAsync("input[name='Password']", "pwd");
        await Page.ClickAsync("text = Log In");
        await Expect(Page.Locator("#loginstatus")).ToHaveTextAsync("Welcome, admin!");
        await Page.ClickAsync("text = Log Out");
        await Expect(Page.Locator("#loginstatus")).ToHaveTextAsync("User logged out.");
        await Page.FillAsync("input[name='UserName']", "admin");   
        await Page.FillAsync("input[name='Password']", "pwdWrong");
        await Page.ClickAsync("text = Log In");
        await Expect(Page.Locator("#loginstatus")).ToHaveTextAsync("Invalid username/password");
        

    }
}
