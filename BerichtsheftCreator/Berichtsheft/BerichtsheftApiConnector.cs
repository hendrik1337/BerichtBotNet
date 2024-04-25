using System.Net;
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
        Console.WriteLine($"Loading lessons from {Environment.GetEnvironmentVariable("crawlerUrl")}");
        using HttpResponseMessage response = await _sharedClient.GetAsync("");

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<List<Lesson>>(jsonResponse);

        Console.WriteLine("Lessons loaded");
        return data;
    }

    public static async void UploadBerichtsheft(String filePath, String fileName, String groupName)
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


                    string folder = $"{nextcloudUrl}{remotePath}/{groupName}";
                    HttpWebRequest httpMkColRequest = (HttpWebRequest)WebRequest.Create(folder);
                    httpMkColRequest.Credentials = new NetworkCredential(username, password);
                    httpMkColRequest.PreAuthenticate = true;

                    httpMkColRequest.Method = @"MKCOL";

                    HttpWebResponse httpMkColResponse = (HttpWebResponse)httpMkColRequest.GetResponse();

                    Console.WriteLine(@"MKCOL Response: {0}", httpMkColResponse.StatusDescription);


                    var response = await client.PutAsync($"{nextcloudUrl}{remotePath}/{groupName}/{fileName}", content);
                    Console.WriteLine($"Uploading to: {nextcloudUrl}{remotePath}/{groupName}/{fileName}");

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