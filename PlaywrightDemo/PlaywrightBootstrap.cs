using System.Diagnostics;

namespace PlaywrightDemo;

public static class PlaywrightBootstrap
{
    // Ensure Playwright browsers are installed by running the generated playwright.ps1 script
    // located in the test output directory. This runs PowerShell and waits for completion.
    public static async Task EnsureInstalledAsync(int timeoutMs = 5 * 60 * 1000, params string[] browsers)
    {
        try
        {
            var outDir = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(outDir, "playwright.ps1");

            if (!File.Exists(scriptPath))
            {
                // Nothing to run (script not present in output) — assume browsers already installed or packaging differs
                return;
            }

            // Try PowerShell Core (pwsh) first, then Windows PowerShell (powershell)
            var candidates = new[] { "pwsh", "powershell" };

            foreach (var exe in candidates)
            {
                try
                {
                    var args = "-NoProfile -ExecutionPolicy Bypass -File \"" + scriptPath + "\" install";
                    if (browsers != null && browsers.Length > 0)
                    {
                        args += " " + string.Join(' ', browsers.Select(b => b));
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = exe,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null)
                        continue;

                    var outputTask = proc.StandardOutput.ReadToEndAsync();
                    var errorTask = proc.StandardError.ReadToEndAsync();

                    var finished = proc.WaitForExit(timeoutMs);
                    var stdout = await outputTask;
                    var stderr = await errorTask;

                    if (finished && proc.ExitCode == 0)
                    {
                        return;
                    }

                    // If the executable is not found, Process.Start will throw; otherwise try next candidate
                }
                catch
                {
                    // ignore and try next
                }
            }
        }
        catch
        {
            // Best-effort only; do not fail tests here — let Playwright produce the clear error if still broken
        }
    }
}
