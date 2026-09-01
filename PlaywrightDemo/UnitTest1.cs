using NUnit.Framework;
using Microsoft.Playwright.NUnit;
using PlaywrightDemo.Pages;

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
        LoginPages loginPages = new LoginPages(page);
        
        await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "screenshot.jpg"
                
            }
        );
        await loginPages.Login("admin", "pwd");
        await loginPages.ClickLogin();
        
        
    }
    
    [Test]
    public async Task GetCallCheck()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        var page = await browser.NewPageAsync();

        var response = await page.RunAndWaitForResponseAsync(
            async () => await page.GotoAsync("https://reqres.in/api/users?page=2"),
            r => r.Url.Contains("api/users?page=2") && r.Request.Method == "GET"
        );

        Assert.That(response.Status, Is.EqualTo(200));

        var json = (await response.JsonAsync())!.Value;
        Assert.That(json.GetProperty("page").GetInt32(), Is.EqualTo(2));
        Assert.That(json.GetProperty("per_page").GetInt32(), Is.EqualTo(6));
        Assert.That(json.GetProperty("total").GetInt32(), Is.EqualTo(12));
        Assert.That(json.GetProperty("total_pages").GetInt32(), Is.EqualTo(2));

        var data = json.GetProperty("data");
        Assert.That(data.GetArrayLength(), Is.EqualTo(6));

        var firstUser = data[0];
        Assert.That(firstUser.GetProperty("id").GetInt32(), Is.EqualTo(7));
        Assert.That(firstUser.GetProperty("email").GetString(), Is.EqualTo("michael.lawson@reqres.in"));
        Assert.That(firstUser.GetProperty("first_name").GetString(), Is.EqualTo("Michael"));
        Assert.That(firstUser.GetProperty("last_name").GetString(), Is.EqualTo("Lawson"));
    }

    [Test]
    public async Task CreatePostCallCheck()
    {
        using var playwright = await Playwright.CreateAsync();
        var request = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = "https://reqres.in"
        });

        var response = await request.PostAsync("/api/users", new() 
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            },
            DataObject = new
            {
                name = "Jango",
                job = "Desuza"
            }
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Status, Is.EqualTo(201));

        var json = await response.JsonAsync();
        Assert.That(json!.Value.GetProperty("name").GetString(), Is.EqualTo("Jango"));
        Assert.That(json.Value.GetProperty("job").GetString(), Is.EqualTo("Desuza"));
        Assert.That(json.Value.GetProperty("id").GetString(), Is.Not.Empty);
        Assert.That(json.Value.GetProperty("createdAt").GetString(), Is.Not.Empty);
    }
}
