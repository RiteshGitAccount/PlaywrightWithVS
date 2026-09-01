namespace PlaywrightDemo.Pages;

public class LoginPages
{
    private readonly IPage _page;
    private readonly ILocator _logIn;
    private readonly ILocator _logout;
    private readonly ILocator _txtUserName;
    private readonly ILocator _txtPassword;
    private readonly ILocator _loginErrorMessage;
    private readonly ILocator _btnLogin;
    private readonly ILocator _apiGetListCall;
    private readonly ILocator _clickSendRequest;
    public LoginPages(IPage page)
    {
        _page = page;
        _btnLogin = _page.Locator("text=Log In");
        _txtUserName = _page.Locator("input[name='UserName']");
        _txtPassword = _page.Locator("input[name='Password']");
        _logIn = _page.Locator("text=Log In");
        _logout = _page.Locator("text=Log Out");
        _loginErrorMessage = _page.Locator("text=Invalid username or password");
        _apiGetListCall = _page.Locator("text=List users");
        _clickSendRequest = _page.Locator("#rp-send-label");

            
            
    }

    public async Task ClickLogin()
    {
        await _logIn.ClickAsync();
    }

    public async Task Login(string username, string password)
    {
        await _txtUserName.FillAsync(username);
        await _txtPassword.FillAsync(password);
    }

    public async Task ClickLinkForAPICall()
    {
        await _apiGetListCall.ClickAsync();
        await _clickSendRequest.ClickAsync();
    }
}