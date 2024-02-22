using System.Net.Http.Headers;
using System.Text.Json;

namespace MauiApp_version;

public partial class App : Application
{
    //private const string GitHubApiUrl = "https://github.com/CCSNidhi/demo/releases/tag/v1.0.0.0";
    private const string GitHubApiUrl = "https://api.github.com/repos/CCSNidhi/demo/releases/latest";

    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
        CheckForUpdates();

    }
    public static async Task<string> CheckForUpdates()
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("demo"); // Set a user agent
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "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");

            HttpResponseMessage response = await client.GetAsync(GitHubApiUrl);
            var limit = response.Headers.GetValues("X-RateLimit-Limit").FirstOrDefault();
            var remaining = response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault();

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubRelease>(json);

                if (release != null && !string.IsNullOrEmpty(release.tag_name))
                {
                    // Compare the retrieved version with your current version
                    var latestVersion = release.tag_name;
                    var currentVersion = "1.0.0.0"; // replace with your actual version

                    if (IsUpdateAvailable(currentVersion, latestVersion))
                    {
                        // Perform the update logic
                        Console.WriteLine($"Update available! New version: {latestVersion}");
                    }
                }
            }
        }

        return null;
    }
    public class GitHubRelease
    {
        public string tag_name { get; set; }
        // Add other properties as needed
    }
    public static bool IsUpdateAvailable(string currentVersion, string latestVersion)
    {
        // Implement version comparison logic
        // Example: Assuming semantic versioning (X.Y.Z)
        var current = Version.Parse(currentVersion);
        var latest = Version.Parse(latestVersion);

        return latest > current;
    }
}
