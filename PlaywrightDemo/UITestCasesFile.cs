using Microsoft.Playwright;
using NUnit.Framework;
using PlaywrightDemo.Pages;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

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
        await page.GotoAsync(TestConfig.BaseUrl + "/sampleapp");

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

    [Test]
    public async Task ProgressBar()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/progressbar");
        await page.ClickAsync("#startButton");
        await page.Locator("#progressBar").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        //click on stop button after progress bar reaches 75%
        //progress bar sometime run very fast and by the time I click stop it reaches above 75%, how to fix this issue, I want to click stop button when progress bar reaches 75%

        // Wait until the progress reaches at least 65% to improve chance stop lands in target range
        await page.WaitForFunctionAsync("() => { const t = document.querySelector('#progressBar').innerText; const m = t.match(/(\\d+)/); return m && parseInt(m[1], 10) >= 75; }");
        await page.ClickAsync("#stopButton");

        // Wait for any UI update and read the progress text
        await page.Locator("#progressBar").WaitForAsync();
        var progressText = await page.Locator("#progressBar").InnerTextAsync();
        var match = System.Text.RegularExpressions.Regex.Match(progressText, "(\\d+)");
        Assert.That(match.Success, Is.True, $"Could not parse progress value from '{progressText}'");

        int percent = int.Parse(match.Groups[1].Value);
        // Assert it's between 65 and 85 (inclusive)
        Assert.That(percent, Is.InRange(70, 80), $"Progress {percent}% is outside expected range 70-80");

    }

    [Test]
    public async Task FileUpload()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();

        await page.GotoAsync(TestConfig.BaseUrl + "/upload");

        var filePath = Path.GetFullPath("TestFile.txt");

        // Find file input in main frame or any child frame
        IElementHandle? inputHandle = await page.QuerySelectorAsync("#browse") ?? await page.QuerySelectorAsync("input[type=file]");
        if (inputHandle == null)
        {
            foreach (var frame in page.Frames)
            {
                inputHandle = await frame.QuerySelectorAsync("#browse") ?? await frame.QuerySelectorAsync("input[type=file]");
                if (inputHandle != null)
                    break;
            }
        }

        if (inputHandle == null)
            throw new InvalidOperationException("File input '#browse' or 'input[type=file]' was not found on the page.");

        await inputHandle.SetInputFilesAsync(filePath);

        var uploadedName = await inputHandle.EvaluateAsync<string>("e => e.files && e.files.length ? e.files[0].name : ''");
        Assert.That(uploadedName, Is.EqualTo(Path.GetFileName(filePath)));
    }

    [Test]
    public async Task Frames()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });

        // Ignore TLS issues on the playground site
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
        var page = await context.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/frames");

        var outerFrameLocator = page.FrameLocator("iframe#frame-outer, iframe[name=frame-outer]");

        // Click the outer "Click me" inside the outer frame
        var outerButton = outerFrameLocator.Locator("button[name=\"my-button\"]");
        await outerButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await outerButton.ClickAsync();
        var outerResult = (await outerFrameLocator.Locator("#result").InnerTextAsync()).Trim();
        Assert.That(outerResult, Is.EqualTo("Button pressed: Click me"), "Outer frame result did not match expected text.");

        // Now click the inner "Click me" inside the inner frame nested within the outer frame
        var innerFrameLocator = outerFrameLocator.FrameLocator("iframe#frame-inner, iframe[name=frame-inner]");
        var innerButton = innerFrameLocator.Locator("button[name=\"my-button\"]");
        await innerButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await innerButton.ClickAsync();
        var innerResult = (await innerFrameLocator.Locator("#result").InnerTextAsync()).Trim();
        Assert.That(innerResult, Is.EqualTo("Button pressed: Click me"), "Inner frame result did not match expected text.");
    }



    [Test]
    public async Task ScrollToClick()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/scrolltoclick");
        // Scroll to the button and click it
        var buttonLocator = page.Locator("#scrollTarget1");
        await buttonLocator.ScrollIntoViewIfNeededAsync();
        await buttonLocator.ClickAsync();
        // Verify that the button was clicked (you can adjust this based on your app's behavior)
        var resultText = await page.Locator("#scrollTarget1").InnerTextAsync();
        Assert.That(resultText, Is.EqualTo("Clicked!"));

        var sideScrollButtonLocator = page.Locator("#scrollTarget2");
        await sideScrollButtonLocator.ScrollIntoViewIfNeededAsync();
        await sideScrollButtonLocator.ClickAsync();
        // Verify that the side button was clicked
        var sideResultText = await page.Locator("#scrollTarget2").InnerTextAsync();
        Assert.That(sideResultText, Is.EqualTo("Clicked!"));

        var bottomScrollButtonLocator = page.Locator("#scrollTarget3");
        await bottomScrollButtonLocator.ScrollIntoViewIfNeededAsync();
        await bottomScrollButtonLocator.ClickAsync();


        var innerScrollButtonLocator = page.Locator("#innerScroll3");
        await innerScrollButtonLocator.ScrollIntoViewIfNeededAsync();
        await innerScrollButtonLocator.ClickAsync();

        var innerResultText = await page.Locator("#scrollTarget3").InnerTextAsync();
        Assert.That(innerResultText, Is.EqualTo("Clicked!"));


        var hoverList = page.Locator("#hoverList");
        //await hoverList.ScrollIntoViewIfNeededAsync();
        // var flagButton =  page.Locator("#scrollTarget4");
        await hoverList.ScrollIntoViewIfNeededAsync();

        await hoverList.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await hoverList.ClickAsync();
        var flagButton = page.Locator("#scrollTarget4");
        await flagButton.ClickAsync();
        await flagButton.HoverAsync();
        var innerFlagButton = page.Locator("#progressText");
        Assert.That(await innerFlagButton.InnerTextAsync(), Is.EqualTo("All buttons clicked!"));

    }

    [Test]
    public async Task ClearInputValues()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/clearinput");
        await page.Locator("#clearInput").ClearAsync();
        await page.Locator("#clearTextarea").ClearAsync();
        await page.Locator("#clearContentEditable").ClearAsync();
    }


    [Test]
    public async Task DisableInput()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/disabledinput");
        await page.Locator("#inputField").FillAsync("Hello Friend");
        await page.Locator("#enableButton").ClickAsync();

        var textValueD = await page.Locator("#opstatus").InnerTextAsync();
        Assert.That(textValueD, Is.EqualTo("Input Disabled..."));
        await Task.Delay(7000);
        var textValueE = await page.Locator("#opstatus").InnerTextAsync();
        Assert.That(textValueE, Is.EqualTo("Input Enabled..."));

    }

    [Test]
    public async Task AnimatedButton()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/animation");
        await page.Locator("#animationButton").ClickAsync();
        await Task.Delay(2000);
        await page.Locator("#movingTarget").ClickAsync();
        var verifyText = await page.Locator("#opstatus").InnerTextAsync();
        Assert.That(verifyText, Is.EqualTo("Moving Target clicked. It's class name is 'btn btn-primary'"));
    }

    [Test]
    public async Task NonBreakingSpace()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/nbsp");
        var button = page.Locator("//button[text()='My&nbsp;Button']");
        await Task.Delay(2000);
        await page.GetByRole(AriaRole.Button, new() { Name = "My Button" }).ClickAsync();
    }

    [Test]
    public async Task ClientSideDelay()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/clientdelay");
        var button = page.GetByRole(AriaRole.Button, new() { Name = "Button Triggering Client Side Logic" }).ClickAsync();
        var message = page.Locator("p.bg-success");

        await Assertions.Expect(message).ToHaveTextAsync("Data calculated on the client side.", new() { Timeout = 30000 });
    }

    [Test]
    public async Task MouseOver()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/mouseover");
        await page.Locator("a.text-primary", new() { HasText = "Click me" }).HoverAsync();
        for (var i = 0; i < 10; i++)
        {
            await page.Locator("a[title='Active Link']").ClickAsync();
            var verifyText = await page.Locator("#clickCount").InnerTextAsync();
            Assert.That(verifyText, Is.EqualTo((i + 1).ToString()));
        }

        await page.Locator("a.text-primary", new() { HasText = "Link Button" }).HoverAsync();
        for (var i = 0; i < 10; i++)
        {
            await page.Locator("a[title='Link Button']").ClickAsync();
            var verifyText = await page.Locator("#clickButtonCount").InnerTextAsync();
            Assert.That(verifyText, Is.EqualTo((i + 1).ToString()));
        }


    }

    [Test]
    public async Task ColorOfHtmlButtonControl()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/hiddenlayers");
        var button = page.Locator("#greenButton");

        string actualColor = await button.EvaluateAsync<string>(
            "element => getComputedStyle(element).backgroundColor"
        );

        await Assertions.Expect(page.Locator("#greenButton")).ToHaveCSSAsync("background-color", "rgb(40, 167, 69)");

        await page.Locator("#greenButton").ClickAsync();

        await Assertions.Expect(page.Locator("#blueButton")).ToHaveCSSAsync("background-color", "rgb(0, 123, 255)");

        Task.Delay(2000).Wait();

    }


    [Test]
    public async Task AlertButton()
    {
        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = false
            });

        var page = await browser.NewPageAsync();

        await page.GotoAsync(TestConfig.BaseUrl + "/alerts");

        // Handle the native browser dialog BEFORE clicking the button.
        page.Dialog += async (_, dialog) =>
        {
            Console.WriteLine($"Dialog Type: {dialog.Type}");
            Console.WriteLine($"Dialog Message: {dialog.Message}");
            Task.Delay(2000).Wait();
            // Accept the confirm/alert dialog
            await dialog.AcceptAsync();
            Task.Delay(2000).Wait(); // Wait for 2 seconds to observe the result
        };

        // This click will now complete because the dialog
        // is automatically accepted when it appears.
        await page.Locator("#confirmButton").ClickAsync();

        Task.Delay(5000).Wait(); // Wait for 2 seconds to observe the result
    }

    [Test]
    public async Task OverlappedElement()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(TestConfig.BaseUrl + "/overlapped");
        var firstTextBox = page.Locator("#id").FillAsync("Hello");
        var secondTextBox = page.Locator("#subject").ScrollIntoViewIfNeededAsync();
        secondTextBox.Wait();
        var enterText = page.Locator("#name").FillAsync("ValueEnteredInSecondTextBox");
        Task.Delay(2000).Wait(); // Wait for 2 seconds to observe the result


    }


}

