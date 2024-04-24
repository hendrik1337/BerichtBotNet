using System.Text;
using BerichtsheftCreator.Data;
using Newtonsoft.Json;

namespace BerichtsheftCreator.Berichtsheft;

public class BerichtsheftApiConnector
{
    private static HttpClient _sharedClient = new()
    {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("crawlerUrl")),
    };

    public static async Task<List<Lesson>?> GetAsync()
    {
        using HttpResponseMessage response = await _sharedClient.GetAsync("");

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<List<Lesson>>(jsonResponse);

        return data;
    }

    public static async void UploadBerichtsheft(String filePath, String fileName)
    {
        {
            string nextcloudUrl = Environment.GetEnvironmentVariable("nextcloudUrl");
            string username = Environment.GetEnvironmentVariable("username");
            string password = Environment.GetEnvironmentVariable("password");
            string remotePath = Environment.GetEnvironmentVariable("remotePath");
            

            using (HttpClient client = new HttpClient())
            {
                // Set Basic Authentication header
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                using (var fileStream = File.OpenRead(filePath))
                {
                    // Upload file
                    var content = new StreamContent(fileStream);
                    var response = await client.PutAsync($"{nextcloudUrl}{remotePath}/{fileName}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("File uploaded successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"File upload failed: {response.StatusCode}");
                    }
                }
            }
        }
    }
}