using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MauiApp_version;

public partial class App : Application
{
    //private const string GitHubApiUrl = "https://github.com/CCSNidhi/demo/releases/tag/v1.0.0.0";
    private const string GitHubApiUrl = "https://api.github.com/repos/CCSNidhi/demo/releases/latest";
    private const string GitHubRepoOwner = "CCSNidhi";
    private const string GitHubRepoName = "demo";
    private const string GitHubApiToken = "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";
   
    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
        CheckForUpdates();

    }
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        if (window != null)
        {
            window.Title = "memori v" + AppInfo.Current.VersionString;
        }
        window.Destroying += Window_Destroying;
        return window;
    }
    private void Window_Destroying(object sender, EventArgs e)
    {
        System.Environment.Exit(0);
    }
    public static async Task<string> CheckForUpdates()
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("demo"); // Set a user agent
           // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "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");

            HttpResponseMessage response = await client.GetAsync(GitHubApiUrl);
            //var limit = response.Headers.GetValues("X-RateLimit-Limit").FirstOrDefault();
            //var remaining = response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault();

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubRelease>(json);

                if (release != null && !string.IsNullOrEmpty(release.tag_name))
                {
                    // Compare the retrieved version with your current version
                    var latestVersion = release.tag_name;
                    var currentVersion = AppInfo.Current.VersionString; // replace with your actual version
                    IsUpdateAvailableAsync(latestVersion, currentVersion);  

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
    public static async Task<bool> IsUpdateAvailableAsync(string currentVersion, string latestVersion)
    {
        if(currentVersion != latestVersion)
        {
            //  App.Current.MainPage = new NavigationPage(new HomePage());
            if (await UpdateManager.DownloadAndInstallUpdate(GitHubRepoOwner, GitHubRepoName, GitHubApiToken))
            {
                
                //Application.Current.CloseWindow();
            }
        }
       
        // Implement version comparison logic
        // Example: Assuming semantic versioning (X.Y.Z)
        //var current = Version.Parse(currentVersion);
        //var latest = Version.Parse(latestVersion);
        
       return true;
    }
}
public static class UpdateManager
{
    
    public static async Task<bool> DownloadAndInstallUpdate(string owner, string repo, string token)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var downloadUrl = $"https://github.com/{owner}/{repo}/releases/latest/download/MauiApp_version_1.2.5.537_x64.msix"; // Replace with the actual URL of your release artifact

                using (var response = await httpClient.GetAsync(downloadUrl))
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string filePath = Path.Combine(appDataDir, "MauiApp_version_1.2.5.537_x64.msix");

                    using (var fileStream = File.Create(filePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                    // Replace "YourMauiApp.zip" with the actual name of your release artifact
                    //using (var fileStream = File.Create("MauiApp_version_1.2.5.537_x64.msix"))
                    //{
                    //    await stream.CopyToAsync(fileStream);
                    //}
                }

                // Extract the downloaded files and replace the existing application files
                // For simplicity, let's assume the update is a ZIP file containing the updated files
                // You would need to implement the extraction logic based on your application structure

                // After extracting, you may need to restart the application
                // For simplicity, let's assume the application executable is named "YourMauiApp.exe"
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"Add-AppPackage -Path .\\MauiApp_version_1.2.5.537_x64.msix",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    process.WaitForExit();

                    return process.ExitCode == 0;
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading and installing update: {ex.Message}");
            return false;
        }
    }
}
