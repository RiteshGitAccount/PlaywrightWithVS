using NUnit.Framework;
using Microsoft.Playwright.NUnit;
using PlaywrightDemo.Pages;

namespace PlaywrightDemo;

public class APITestCasesFile
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task UILoginSampleTestCase()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.Url("/sampleapp"));
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
    public async Task PostCallCheck()
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

    [Test]
    public async Task UpdateCallCheck()
    {
        using var playwright = await Playwright.CreateAsync();
        var request = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = "https://reqres.in"
        });

        var response = await request.PatchAsync("/api/users/2", new()
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            },
            DataObject = new
            {
                job = "DesuzaTest"
            }
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Status, Is.EqualTo(200));

        var json = await response.JsonAsync();
        Assert.That(json.Value.GetProperty("job").GetString(), Is.EqualTo("DesuzaTest"));
    }

    [Test]
    public async Task DeleteCallCheck()
    {
        using var playwright = await Playwright.CreateAsync();
        var request = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = "https://reqres.in"
        });

        var response = await request.DeleteAsync("/api/users/2");

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task PutCallCheck()
    {
        using var playwright = await Playwright.CreateAsync();
        var request = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = "https://reqres.in"
        });
        var response = await request.PutAsync("/api/users/2", new()
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            },
            DataObject = new
            {
                name = "PutJango",
                job = "PutDesuza"
            }
        });
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        Assert.That(json!.Value.GetProperty("name").GetString(), Is.EqualTo("PutJango"));
        Assert.That(json.Value.GetProperty("job").GetString(), Is.EqualTo("PutDesuza"));
    }

    [Test]
    public async Task PostCallRegisterUser()
    {
        using var playwright = await Playwright.CreateAsync();
        var request = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = "https://reqres.in"
        });

        var response = await request.PostAsync("/api/register", new()
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            },
            DataObject = new
            {
                email = "eve.holt@reqres.in",
                password = "pistol"
            }
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Status, Is.EqualTo(200));

        var json = await response.JsonAsync();

        // Validate top-level fields
        Assert.That(json!.Value.GetProperty("id").GetInt32(), Is.EqualTo(4));
        Assert.That(json.Value.GetProperty("token").GetString(), Is.EqualTo("QpwL5tke4Pnpja7X4"));

        // Validate _meta object
        var meta = json.Value.GetProperty("_meta");
        Assert.That(meta.GetProperty("powered_by").GetString(), Is.EqualTo("ReqRes"));
        Assert.That(meta.GetProperty("docs_url").GetString(), Is.EqualTo("https://app.reqres.in/documentation"));
        Assert.That(meta.GetProperty("upgrade_url").GetString(), Is.EqualTo("https://app.reqres.in/upgrade"));
        Assert.That(meta.GetProperty("example_url").GetString(), Is.EqualTo("https://app.reqres.in/examples/notes-app"));
        Assert.That(meta.GetProperty("variant").GetString(), Is.EqualTo("v1_a"));
        Assert.That(meta.GetProperty("message").GetString(), Is.EqualTo("Your data persists here. Add auth, logs, and custom schemas to build a real backend."));
        var cta = meta.GetProperty("cta");
        Assert.That(cta.GetProperty("label").GetString(), Is.EqualTo("See example app"));
        Assert.That(cta.GetProperty("url").GetString(), Is.EqualTo("https://app.reqres.in/examples/notes-app"));
        Assert.That(meta.GetProperty("context").GetString(), Is.EqualTo("legacy_success"));
    }
}