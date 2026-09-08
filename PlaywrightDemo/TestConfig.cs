namespace PlaywrightDemo;

public static class TestConfig
{
    // Base URL for the UITesting Playground. Change here to affect all tests.
    public static string BaseUrl { get; set; } = "http://uitestingplayground.com";

    // Returns a full URL for a given endpoint path. Accepts leading or non-leading '/'.
    public static string Url(string path)
    {
        if (string.IsNullOrEmpty(path))
            return BaseUrl;

        return path.StartsWith("/") ? BaseUrl + path : BaseUrl + "/" + path;
    }
}
